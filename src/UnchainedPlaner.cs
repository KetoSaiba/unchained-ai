using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WindBot.Game.AI.Decks
{
    /// <summary>Kartenarten des Decks. Reihenfolge: Main-Deck-Monster, Zauber/Fallen, Extra Deck.</summary>
    internal enum K : byte
    {
        Shma, Shara, Kamura, Aruha, Sarama, Rakea, Sharvara, Shyama, Disaster, Abominable, Malice, Banish,
        Prison, Reinc, Wailing, Chamber, Escape, Syncretism, Imperm, Evenly,
        SoulLord, Rage, Anguish, Abominator, Enma, Abomination, Anderes, N
    }

    /// <summary>Wo eine Karte liegt, die ein Plan-Schritt auswählt.</summary>
    internal enum O : byte { Hand, Deck, Grab, Monster, Zone, GegnerM, GegnerZ, Extra }

    internal struct Wahl
    {
        public K Karte; public O Ort; public byte Verdeckt; public short Gegner;
        public Wahl(K k, O o, bool verdeckt = false, int gegner = -1) { Karte = k; Ort = o; Verdeckt = (byte)(verdeckt ? 1 : 0); Gegner = (short)gegner; }
        public override string ToString() => Gegner >= 0 ? $"Gegner#{Gegner}" : $"{Karte}@{Ort}{(Verdeckt != 0 ? "(v)" : "")}";
    }

    internal enum Art : byte { Normal, Setzen, Aktivieren, Link, Ausloeser, Auslassen }

    /// <summary>Ein Schritt des Plans: Art, Karte, Ort der Aktivierung und die Auswahl in Reihenfolge der Abfragen.</summary>
    internal sealed class Schritt
    {
        public Art Art; public K Karte; public O Ort; public int Gruppe;
        public Wahl[] Wahlen = Array.Empty<Wahl>();
        public bool Ja;              // „dann …?“-Frage bejahen (Shyama, Soul Lord, Reincarnation, Kamura-Falle)
        public bool Beschwoeren;     // Shara: beschwören statt auf die Hand
        public bool Ziehen;          // Kamura zieht: danach neu planen
        public override string ToString() => $"{Art} {Karte}@{Ort}" + (Wahlen.Length > 0 ? " [" + string.Join(", ", Wahlen) + "]" : "") + (Ja ? " ja" : "") + (Gruppe > 0 ? $" G{Gruppe}" : "");
    }

    /// <summary>Gegnerkarte für den Planer: Wert und ob sie die eigenen Effekte ignoriert.</summary>
    internal sealed class GegnerKarte { public int Id, Wert, Atk, Link; public bool Offen, Immun, Unzerst, Monster, Verteidigung; }

    /// <summary>Zugplaner: modelliert den Zerstörungs-Motor des Decks und sucht per Strahlsuche die beste Zugfolge für den aktuellen
    /// Zustand. Jeder Effekt nur 1× pro Zug, jede Zerstörung sollte einen noch freien Auslöser treffen; das lässt sich nicht als feste
    /// Regeln schreiben, weil Starthand und Handfallen die Reihenfolge ändern. Stimmt das echte Spiel nicht mehr mit dem Plan
    /// überein (Handfalle, gezogene Karte), plant die KI vom echten Zustand neu.</summary>
    internal sealed class Planer
    {
        internal const int NK = (int)K.N;
        // --- Einmal-pro-Zug-Effekte ---------------------------------------------------------------
        internal enum E
        {
            ShmaNS, ShmaZ, SharaHand, SharaGrab, KamuraHand, KamuraZ, AruhaHand, AruhaZ, SaramaFeld, SaramaZ, RakeaFeld, RakeaZ,
            SharvaraHand, SharvaraGrab, ShyamaFeld, ShyamaGrab, DisasterLink, DisasterZ, AbomSS, AbomPop, MaliceAdd, BanishAdd,
            PrisonAkt, PrisonZ, ReincAkt, ReincFeld, WailingLink, WailingZ, ChamberAkt, ChamberZ, EscapeAkt, EscapeZ, SyncAkt, SyncZ,
            SoulSuche, SoulGrab, RageZ, AnguishLink, AnguishZ, EnmaEnde, EnmaErsatz, AbominationZ, Anzahl
        }
        // --- Auslöser in der Warteschlange ---------------------------------------------------------
        private enum T : byte
        {
            ShmaNS, ShmaZ, AruhaZ, SaramaZ, RakeaZ, PrisonZ, WailingZ, ChamberZ, EscapeZ, SyncZ, KamuraBombe, DisasterZ, SharvaraGrab,
            RageZ, AnguishZ, SharaGrab, SoulGrab, AbomHand, AbomPop, SoulSuche, WailingLink, MaliceAdd, BanishAdd, AbominationZ
        }

        private const byte FNegiert = 1, FBoden = 2, FVerdeckt = 4;   // Monster-/Zonen-Flags
        private const byte ZVerdeckt = 1, ZNeu = 2;                   // Zonen: verdeckt, erst diesen Zug gesetzt

        // Aufbau des Puffers: Hand, Deck/Extra, Friedhof, Friedhof-durch-Zerstörung, dann Felder fester Länge
        private const int OH = 0, OD = NK, OG = 2 * NK, OZ = 3 * NK, OM = 4 * NK, MaxM = 7, OMF = OM + MaxM, OS = OMF + MaxM, MaxS = 6, OSF = OS + MaxS,
            OP = OSF + MaxS, MaxP = 8, OQ = OP + MaxP, OC = OQ + MaxP, CM = OC, CS = OC + 1, CP = OC + 2, CQ = OC + 3, CNS = OC + 4, CGez = OC + 5,
            CGegM = OC + 6, CGegZ = OC + 7, CLinkOK = OC + 8, Laenge = OC + 9;

        internal sealed class Z
        {
            public byte[] B = new byte[Laenge];
            public ulong U;
            public Z Vater; public Schritt Schritt; public int Gruppe;
            public Z Kopie(Schritt s)
            {
                var z = new Z { U = U, Vater = this, Schritt = s, Gruppe = Gruppe };
                Buffer.BlockCopy(B, 0, z.B, 0, Laenge);
                return z;
            }
            public ulong Hash()
            {
                ulong h = 14695981039346656037UL ^ U;
                foreach (byte x in B) { h ^= x; h *= 1099511628211UL; }
                return h;
            }
        }

        // --- Eigenschaften der Karten -------------------------------------------------------------
        private static readonly K[] Monster = { K.Shma, K.Shara, K.Kamura, K.Aruha, K.Sarama, K.Rakea, K.Sharvara, K.Shyama, K.Disaster, K.Abominable };
        private static readonly K[] UnchainedZF = { K.Reinc, K.Wailing, K.Chamber, K.Escape, K.Syncretism };
        private static bool IstUnchainedMonster(K k) => k <= K.Abominable;
        private static bool IstLink(K k) => k >= K.SoulLord && k <= K.Abomination;
        private static bool IstSeele(K k) => k == K.Sharvara || k == K.Shyama || k == K.Disaster || k == K.Abominable || k == K.SoulLord || k == K.Rage || k == K.Anguish || k == K.Abominator;
        private static bool IstUnchained(K k) => IstUnchainedMonster(k) || UnchainedZF.Contains(k) || IstLink(k);
        private static bool IstSetzAusloeser(K k) => k == K.Prison || k == K.Wailing || k == K.Chamber || k == K.Escape || k == K.Syncretism;
        private static int LinkWert(K k) => k == K.SoulLord || k == K.Rage ? 2 : k == K.Anguish || k == K.Abominator ? 3 : k == K.Enma || k == K.Abomination ? 4 : 1;
        private static bool IstStufe8(K k) => k == K.Kamura || k == K.Disaster || k == K.Abominable;
        private static int Stufe(K k) => k == K.Shma || k == K.Shara ? 4 : k == K.Aruha || k == K.Sarama || k == K.Rakea || k == K.Malice || k == K.Banish ? 3 : 8;

        private readonly List<GegnerKarte> _gegner;
        private readonly bool _ersterZug;
        private readonly int _gegnerLP;
        private readonly HashSet<string> _verboten;
        private readonly bool _hauptphase2;
        private readonly bool _gegnerzug, _hauptOffen;   // Gegnerzug: nur schnelle Effekte, Sharvara/Shara nur in der Main Phase
        private int _knoten;

        internal Planer(List<GegnerKarte> gegner, bool ersterZug, int gegnerLP, HashSet<string> verboten, bool hauptphase2, bool gegnerzug = false, bool hauptOffen = true)
        {
            _gegnerzug = gegnerzug; _hauptOffen = hauptOffen;
            _gegner = gegner; _ersterZug = ersterZug; _gegnerLP = gegnerLP; _verboten = verboten; _hauptphase2 = hauptphase2;
            GegnerMonster = _gegner.Where(g => g.Monster).Take(8).ToList();
            GegnerZonen = _gegner.Where(g => !g.Monster).Take(8).ToList();
        }

        internal int Knoten => _knoten;

        internal static Z Leer() => new Z();
        internal static void SetHand(Z z, K k, int n) => z.B[OH + (int)k] = (byte)n;
        internal static void SetDeck(Z z, K k, int n) => z.B[OD + (int)k] = (byte)Math.Max(0, n);
        internal static void AddGrab(Z z, K k, bool zerstoert) { z.B[OG + (int)k]++; if (zerstoert) z.B[OZ + (int)k]++; }
        internal static void AddMonster(Z z, K k, bool negiert, bool boden, bool verdeckt)
        {
            int i = z.B[CM]++; z.B[OM + i] = (byte)k;
            z.B[OMF + i] = (byte)((negiert ? FNegiert : 0) | (boden ? FBoden : 0) | (verdeckt ? FVerdeckt : 0));
        }
        internal static void AddZone(Z z, K k, bool verdeckt, bool neu)
        {
            int i = z.B[CS]++; z.B[OS + i] = (byte)k;
            z.B[OSF + i] = (byte)((verdeckt ? ZVerdeckt : 0) | (neu ? ZNeu : 0));
        }
        internal static void NormalVerbraucht(Z z) => z.B[CNS] = 1;
        internal static void Benutzt(Z z, E e) => z.U |= 1UL << (int)e;
        internal static bool IstBenutzt(Z z, E e) => (z.U & (1UL << (int)e)) != 0;
        internal void SetGegner(Z z)
        {
            int m = 0, s = 0, bm = 0, bs = 0;
            for (int i = 0; i < _gegner.Count; i++)
            {
                if (_gegner[i].Monster) { if (m < 8) bm |= 1 << m; m++; }
                else { if (s < 8) bs |= 1 << s; s++; }
            }
            z.B[CGegM] = (byte)bm; z.B[CGegZ] = (byte)bs;
        }
        private readonly List<GegnerKarte> GegnerMonster, GegnerZonen;

        /// <summary>Vergleichswert für „stimmt das Spiel noch mit dem Plan überein“: Hand, Friedhof, Feld (ohne Deck).</summary>
        internal static string Signatur(Z z)
        {
            var t = new System.Text.StringBuilder();
            for (int k = 0; k < NK; k++) if (z.B[OH + k] > 0) t.Append('H').Append((K)k).Append(z.B[OH + k]);
            t.Append('|');
            for (int k = 0; k < NK; k++) if (z.B[OG + k] > 0) t.Append('G').Append((K)k).Append(z.B[OG + k]);
            t.Append('|');
            var m = new List<string>();
            for (int i = 0; i < z.B[CM]; i++) m.Add(((K)z.B[OM + i]).ToString() + ((z.B[OMF + i] & FVerdeckt) != 0 ? "v" : ""));
            m.Sort(StringComparer.Ordinal);
            t.Append(string.Join(",", m)).Append('|');
            var zo = new List<string>();
            for (int i = 0; i < z.B[CS]; i++) zo.Add(((K)z.B[OS + i]).ToString() + ((z.B[OSF + i] & ZVerdeckt) != 0 ? "v" : ""));
            zo.Sort(StringComparer.Ordinal);
            t.Append(string.Join(",", zo));
            return t.ToString();
        }

        // =====================================================================================
        // Grundoperationen
        // =====================================================================================

        private static bool Frei(Z z, E e) => (z.U & (1UL << (int)e)) == 0;
        private static void Nutze(Z z, E e) => z.U |= 1UL << (int)e;
        private static int MainBelegt(Z z)
        {
            int n = 0, links = 0;
            for (int i = 0; i < z.B[CM]; i++)
            {
                if (IstLink((K)z.B[OM + i])) links++; else n++;
            }
            return n + Math.Max(0, links - 1);       // ein Link steht in der Extra-Monsterzone
        }
        private static bool ZoneFrei(Z z) => MainBelegt(z) < 5;
        private static bool STFrei(Z z) => z.B[CS] < 5;
        private static int FeldZahl(Z z) => z.B[CM] + z.B[CS];

        private static void MonsterRein(Z z, K k, byte flags = 0)
        {
            int i = z.B[CM]++; z.B[OM + i] = (byte)k; z.B[OMF + i] = flags;
        }
        private static void MonsterRaus(Z z, int i)
        {
            int last = --z.B[CM];
            for (int j = i; j < last; j++) { z.B[OM + j] = z.B[OM + j + 1]; z.B[OMF + j] = z.B[OMF + j + 1]; }
        }
        private static void ZoneRaus(Z z, int i)
        {
            int last = --z.B[CS];
            for (int j = i; j < last; j++) { z.B[OS + j] = z.B[OS + j + 1]; z.B[OSF + j] = z.B[OSF + j + 1]; }
        }
        private static void Neu(Z z, T t)
        {
            for (int i = 0; i < z.B[CP]; i++) if (z.B[OP + i] == (byte)t) return;
            for (int i = 0; i < z.B[CQ]; i++) if (z.B[OQ + i] == (byte)t) return;
            if (z.B[CQ] < MaxP) z.B[OQ + z.B[CQ]++] = (byte)t;
        }
        /// <summary>Nach einer Aktion: neue Auslöser bilden die nächste Kette, sobald die laufende aufgelöst ist.</summary>
        private static void Schalten(Z z)
        {
            if (z.B[CP] == 0 && z.B[CQ] > 0)
            {
                for (int i = 0; i < z.B[CQ]; i++) z.B[OP + i] = z.B[OQ + i];
                z.B[CP] = z.B[CQ]; z.B[CQ] = 0;
                z.Gruppe++;
            }
        }

        private void ZumFriedhof(Z z, K k, bool zerstoert)
        {
            z.B[OG + (int)k]++;
            if (zerstoert) z.B[OZ + (int)k]++;
            if (k == K.Sharvara && Frei(z, E.SharvaraGrab)) Neu(z, T.SharvaraGrab);
            if (IstStufe8(k))
            {
                if (z.B[OG + (int)K.Malice] > 0 && Frei(z, E.MaliceAdd)) Neu(z, T.MaliceAdd);
                if (z.B[OG + (int)K.Banish] > 0 && Frei(z, E.BanishAdd)) Neu(z, T.BanishAdd);
            }
        }

        /// <summary>Zerstört eigene Karten (Monster-/Zonen-Indizes) als eine Gruppe. quelle = Karte, deren Effekt zerstört.</summary>
        private void Zerstoere(Z z, List<int> monster, List<int> zonen, K quelle)
        {
            bool shara = z.B[OG + (int)K.Shara] > 0, soul = z.B[OG + (int)K.SoulLord] > 0;
            var mk = monster.Select(i => ((K)z.B[OM + i], z.B[OMF + i])).ToList();
            var zk = zonen.Select(i => ((K)z.B[OS + i], z.B[OSF + i])).ToList();
            foreach (int i in monster.OrderByDescending(i => i)) MonsterRaus(z, i);
            foreach (int i in zonen.OrderByDescending(i => i)) ZoneRaus(z, i);
            foreach (var (k, f) in mk)
            {
                if ((f & FBoden) != 0) { z.B[OD + (int)k]++; continue; }  // „unter das Deck, wenn es das Feld verlässt“
                ZumFriedhof(z, k, true);
                switch (k)
                {
                    case K.Shma: if (quelle != K.Shma && Frei(z, E.ShmaZ)) Neu(z, T.ShmaZ); break;
                    case K.Aruha: if (quelle != K.Aruha && Frei(z, E.AruhaZ)) Neu(z, T.AruhaZ); break;
                    case K.Sarama: if (quelle != K.Sarama && Frei(z, E.SaramaZ)) Neu(z, T.SaramaZ); break;
                    case K.Rakea: if (quelle != K.Rakea && Frei(z, E.RakeaZ)) Neu(z, T.RakeaZ); break;
                    case K.Kamura: if (Frei(z, E.KamuraZ)) Neu(z, T.KamuraBombe); break;
                    case K.Disaster: if (Frei(z, E.DisasterZ)) Neu(z, T.DisasterZ); break;
                    case K.Rage: if (Frei(z, E.RageZ)) Neu(z, T.RageZ); break;
                    case K.Anguish: if (Frei(z, E.AnguishZ)) Neu(z, T.AnguishZ); break;
                }
            }
            foreach (var (k, f) in zk)
            {
                ZumFriedhof(z, k, true);
                if ((f & ZVerdeckt) == 0) continue;
                switch (k)
                {
                    case K.Prison: if (Frei(z, E.PrisonZ)) Neu(z, T.PrisonZ); break;
                    case K.Wailing: if (Frei(z, E.WailingZ)) Neu(z, T.WailingZ); break;
                    case K.Chamber: if (Frei(z, E.ChamberZ)) Neu(z, T.ChamberZ); break;
                    case K.Escape: if (Frei(z, E.EscapeZ)) Neu(z, T.EscapeZ); break;
                    case K.Syncretism: if (Frei(z, E.SyncZ)) Neu(z, T.SyncZ); break;
                }
            }
            if (mk.Count + zk.Count == 0) return;
            if (soul && !mk.Any(m => m.Item1 == K.SoulLord) && Frei(z, E.SoulGrab)) Neu(z, T.SoulGrab);
            if (shara && !mk.Any(m => m.Item1 == K.Shara) && quelle != K.Shara && Frei(z, E.SharaGrab)) Neu(z, T.SharaGrab);
            if (z.B[OH + (int)K.Abominable] > 0 && Frei(z, E.AbomSS)) Neu(z, T.AbomHand);
            if (quelle != K.Abomination && Frei(z, E.AbominationZ))
                for (int i = 0; i < z.B[CM]; i++)
                    if ((K)z.B[OM + i] == K.Abomination) { Neu(z, T.AbominationZ); break; }
        }

        /// <summary>Zerstörbare eigene Karten, je Art nur einmal (gleiche Karten führen zum selben Zustand).</summary>
        private static void Ziele(Z z, List<(bool monster, int i)> liste, bool nurFiendOderVerdeckt = false, bool ohneBoss = true)
        {
            liste.Clear();
            int gesehen = 0;
            for (int i = 0; i < z.B[CM]; i++)
            {
                K k = (K)z.B[OM + i]; byte f = z.B[OMF + i];
                if (ohneBoss && (k == K.Enma || k == K.Abomination)) continue;
                int key = (int)k * 8 + f;
                bool dop = false;
                for (int j = 0; j < i; j++) if ((int)z.B[OM + j] * 8 + z.B[OMF + j] == key) dop = true;
                if (dop) continue;
                liste.Add((true, i)); gesehen++;
            }
            for (int i = 0; i < z.B[CS]; i++)
            {
                byte f = z.B[OSF + i];
                if (nurFiendOderVerdeckt && (f & ZVerdeckt) == 0) continue;
                int key = z.B[OS + i] * 8 + f;
                bool dop = false;
                for (int j = 0; j < i; j++) if (z.B[OS + j] * 8 + z.B[OSF + j] == key) dop = true;
                if (dop) continue;
                liste.Add((false, i));
            }
        }
        private static Wahl WahlFeld(Z z, (bool monster, int i) t) => t.monster
            ? new Wahl((K)z.B[OM + t.i], O.Monster, (z.B[OMF + t.i] & FVerdeckt) != 0)
            : new Wahl((K)z.B[OS + t.i], O.Zone, (z.B[OSF + t.i] & ZVerdeckt) != 0);
        private void Zerstoere1(Z z, (bool monster, int i) t, K quelle)
        {
            if (t.monster) Zerstoere(z, new List<int> { t.i }, new List<int>(), quelle);
            else Zerstoere(z, new List<int>(), new List<int> { t.i }, quelle);
        }

        /// <summary>Monster dieser Art aus Hand oder Deck (Deck zuerst: dünnt aus und lässt die Handkarte für ihren Effekt).</summary>
        private static bool HoleHandDeck(Z z, K k, out O ort, bool nurDeck = false)
        {
            ort = O.Deck;
            if (z.B[OD + (int)k] > 0) { z.B[OD + (int)k]--; return true; }
            if (nurDeck || z.B[OH + (int)k] == 0) return false;
            z.B[OH + (int)k]--; ort = O.Hand;
            return true;
        }

        // =====================================================================================
        // Züge erzeugen
        // =====================================================================================

        private readonly List<(bool monster, int i)> _z = new List<(bool, int)>();
        private readonly List<(bool monster, int i)> _z2 = new List<(bool, int)>();

        private void Kind(List<Z> aus, Z z)
        {
            Schalten(z);
            aus.Add(z);
        }

        private bool Erlaubt(Schritt s) => _verboten == null || _verboten.Count == 0 || !_verboten.Contains(s.Art + " " + s.Karte + " " + s.Ort);

        internal void Erweitere(Z v, List<Z> aus)
        {
            if (v.B[CP] > 0) { Ausloeser(v, aus); return; }
            if (_gegnerzug) { Schnell(v, aus); return; }

            // Normalbeschwörung
            if (v.B[CNS] == 0)
            {
                foreach (K k in new[] { K.Shma, K.Rakea, K.Aruha, K.Sarama, K.Shara, K.Malice, K.Banish })
                {
                    if (v.B[OH + (int)k] == 0 || !ZoneFrei(v)) continue;
                    var s = new Schritt { Art = Art.Normal, Karte = k, Ort = O.Hand };
                    if (!Erlaubt(s)) continue;
                    var z = v.Kopie(s);
                    z.B[OH + (int)k]--; z.B[CNS] = 1;
                    MonsterRein(z, k);
                    if (k == K.Shma && Frei(z, E.ShmaNS)) Neu(z, T.ShmaNS);
                    Kind(aus, z);
                }
            }

            // Zauber und Fallen von der Hand
            if (STFrei(v))
            {
                if (v.B[OH + (int)K.Prison] > 0 && Frei(v, E.PrisonAkt)) PrisonAktivieren(v, aus, O.Hand, -1);
                if (v.B[OH + (int)K.Reinc] > 0 && Frei(v, E.ReincAkt)) ReincAktivieren(v, aus, O.Hand, -1);
                if (v.B[OH + (int)K.Wailing] > 0 && !ZoneMit(v, K.Wailing, false))
                {
                    var s = new Schritt { Art = Art.Aktivieren, Karte = K.Wailing, Ort = O.Hand };
                    if (Erlaubt(s))
                    {
                        var z = v.Kopie(s);
                        z.B[OH + (int)K.Wailing]--; z.B[OS + z.B[CS]] = (byte)K.Wailing; z.B[OSF + z.B[CS]] = 0; z.B[CS]++;
                        Kind(aus, z);
                    }
                }
                foreach (K k in new[] { K.Prison, K.Chamber, K.Escape, K.Syncretism, K.Wailing, K.Evenly, K.Imperm })
                {
                    if (v.B[OH + (int)k] == 0) continue;
                    var s = new Schritt { Art = Art.Setzen, Karte = k, Ort = O.Hand };
                    if (!Erlaubt(s)) continue;
                    var z = v.Kopie(s);
                    z.B[OH + (int)k]--; z.B[OS + z.B[CS]] = (byte)k; z.B[OSF + z.B[CS]] = ZVerdeckt | ZNeu; z.B[CS]++;
                    Kind(aus, z);
                }
            }
            // Gesetzte Karten aktivieren
            for (int i = 0; i < v.B[CS]; i++)
            {
                K k = (K)v.B[OS + i]; byte f = v.B[OSF + i];
                if ((f & ZVerdeckt) == 0)
                {
                    if (k == K.Reinc && Frei(v, E.ReincFeld)) ReincFeld(v, aus);
                    continue;
                }
                if (k == K.Reinc && Frei(v, E.ReincAkt)) ReincAktivieren(v, aus, O.Zone, i);
                if (k == K.Prison && Frei(v, E.PrisonAkt)) PrisonAktivieren(v, aus, O.Zone, i);
                if ((f & ZNeu) != 0) continue;     // Fallen erst im nächsten Zug
                if (k == K.Chamber && Frei(v, E.ChamberAkt)) ChamberAktivieren(v, aus, i);
                if (k == K.Escape && Frei(v, E.EscapeAkt)) EscapeAktivieren(v, aus, i);
            }

            // Handeffekte
            if (v.B[OH + (int)K.Aruha] > 0 && Frei(v, E.AruhaHand)) HandZerstoerer(v, aus, K.Aruha, E.AruhaHand, false);
            if (v.B[OH + (int)K.Sharvara] > 0 && Frei(v, E.SharvaraHand)) HandZerstoerer(v, aus, K.Sharvara, E.SharvaraHand, true);
            if (v.B[OH + (int)K.Kamura] > 0 && Frei(v, E.KamuraHand)) KamuraHand(v, aus);
            if (v.B[OH + (int)K.Shara] > 0 && Frei(v, E.SharaHand)) SharaHand(v, aus);

            // Feldeffekte
            for (int i = 0; i < v.B[CM]; i++)
            {
                K k = (K)v.B[OM + i]; byte f = v.B[OMF + i];
                if ((f & (FNegiert | FVerdeckt)) != 0) continue;
                if (k == K.Rakea && Frei(v, E.RakeaFeld)) FeldZerstoerer(v, aus, K.Rakea, E.RakeaFeld);
                if (k == K.Shyama && Frei(v, E.ShyamaFeld)) ShyamaFeld(v, aus);
                if (k == K.Sarama && Frei(v, E.SaramaFeld)) SaramaFeld(v, aus);
                if (k == K.Disaster && Frei(v, E.DisasterLink)) GegnerLink(v, aus, i, K.Disaster, E.DisasterLink);
                if (k == K.Anguish && Frei(v, E.AnguishLink)) GegnerLink(v, aus, i, K.Anguish, E.AnguishLink);
            }
            // Friedhof
            if (v.B[OG + (int)K.Shyama] > 0 && Frei(v, E.ShyamaGrab)) HandZerstoerer(v, aus, K.Shyama, E.ShyamaGrab, true, O.Grab);

            // Links
            LinkZuege(v, aus);
        }

        /// <summary>Gegnerzug: nur schnelle Effekte (Rakea, Sharvara, Shara, gesetzte Fallen). Die Auslöser laufen wie im eigenen Zug.</summary>
        private void Schnell(Z v, List<Z> aus)
        {
            for (int i = 0; i < v.B[CM]; i++)
                if ((K)v.B[OM + i] == K.Rakea && (v.B[OMF + i] & (FNegiert | FVerdeckt)) == 0 && Frei(v, E.RakeaFeld))
                {
                    FeldZerstoerer(v, aus, K.Rakea, E.RakeaFeld);
                    break;
                }
            if (_hauptOffen)
            {
                if (v.B[OH + (int)K.Sharvara] > 0 && Frei(v, E.SharvaraHand)) HandZerstoerer(v, aus, K.Sharvara, E.SharvaraHand, true);
                if (v.B[OH + (int)K.Shara] > 0 && Frei(v, E.SharaHand)) SharaHand(v, aus);
            }
            for (int i = 0; i < v.B[CS]; i++)
            {
                K k = (K)v.B[OS + i]; byte f = v.B[OSF + i];
                if ((f & ZVerdeckt) == 0 || (f & ZNeu) != 0) continue;
                if (k == K.Chamber && Frei(v, E.ChamberAkt)) ChamberAktivieren(v, aus, i);
                if (k == K.Escape && Frei(v, E.EscapeAkt)) EscapeAktivieren(v, aus, i);
            }
        }

        private static bool ZoneMit(Z z, K k, bool verdeckt)
        {
            for (int i = 0; i < z.B[CS]; i++) if ((K)z.B[OS + i] == k && ((z.B[OSF + i] & ZVerdeckt) != 0) == verdeckt) return true;
            return false;
        }

        private void PrisonAktivieren(Z v, List<Z> aus, O ort, int zone)
        {
            foreach (K k in Monster.Concat(UnchainedZF))
            {
                if (v.B[OD + (int)k] == 0) continue;
                var s = new Schritt { Art = Art.Aktivieren, Karte = K.Prison, Ort = ort, Wahlen = new[] { new Wahl(k, O.Deck) } };
                if (!Erlaubt(s)) continue;
                var z = v.Kopie(s);
                if (ort == O.Hand) z.B[OH + (int)K.Prison]--; else ZoneRaus(z, zone);
                Nutze(z, E.PrisonAkt);
                z.B[OG + (int)K.Prison]++;
                z.B[OD + (int)k]--; z.B[OH + (int)k]++;
                Kind(aus, z);
            }
        }

        private void ReincAktivieren(Z v, List<Z> aus, O ort, int zone)
        {
            foreach (K k in new[] { K.Prison, K.Wailing })
            {
                if (v.B[OD + (int)k] == 0) continue;
                var s = new Schritt { Art = Art.Aktivieren, Karte = K.Reinc, Ort = ort, Ja = true, Wahlen = new[] { new Wahl(k, O.Deck) } };
                if (!Erlaubt(s)) continue;
                var z = v.Kopie(s);
                if (ort == O.Hand) { z.B[OH + (int)K.Reinc]--; z.B[OS + z.B[CS]] = (byte)K.Reinc; z.B[OSF + z.B[CS]] = 0; z.B[CS]++; }
                else z.B[OSF + zone] = 0;
                Nutze(z, E.ReincAkt);
                z.B[OD + (int)k]--; z.B[OH + (int)k]++;
                Kind(aus, z);
            }
        }

        /// <summary>Reincarnation offen: bis zu 3 Unchained-Karten zurück ins Deck, dann bis zu so viele eigene Karten zerstören.
        /// Für die Suche: 1 Karte zurück + 1 Karte zerstören.</summary>
        private void ReincFeld(Z v, List<Z> aus)
        {
            var grab = Monster.Concat(UnchainedZF).Where(k => k != K.Reinc && v.B[OG + (int)k] > 0).ToList();
            if (grab.Count == 0) return;
            Ziele(v, _z2);
            foreach (K k in grab)
            {
                foreach (var t in _z2)
                {
                    if (!t.monster && (K)v.B[OS + t.i] == K.Reinc && (v.B[OSF + t.i] & ZVerdeckt) == 0) continue;
                    var s = new Schritt { Art = Art.Aktivieren, Karte = K.Reinc, Ort = O.Zone, Ja = true, Wahlen = new[] { new Wahl(k, O.Grab), WahlFeld(v, t) } };
                    if (!Erlaubt(s)) continue;
                    var z = v.Kopie(s);
                    Nutze(z, E.ReincFeld);
                    z.B[OG + (int)k]--; if (z.B[OZ + (int)k] > z.B[OG + (int)k]) z.B[OZ + (int)k] = z.B[OG + (int)k];
                    z.B[OD + (int)k]++;
                    Zerstoere1(z, t, K.Reinc);
                    Kind(aus, z);
                }
            }
        }

        private void ChamberAktivieren(Z v, List<Z> aus, int zone)
        {
            foreach (K k in Monster)
            {
                foreach (O o in new[] { O.Hand, O.Grab })
                {
                    int off = o == O.Hand ? OH : OG;
                    if (v.B[off + (int)k] == 0 || !ZoneFrei(v)) continue;
                    var s = new Schritt { Art = Art.Aktivieren, Karte = K.Chamber, Ort = O.Zone, Wahlen = new[] { new Wahl(k, o) } };
                    if (!Erlaubt(s)) continue;
                    var z = v.Kopie(s);
                    ZoneRaus(z, zone);
                    Nutze(z, E.ChamberAkt);
                    z.B[OG + (int)K.Chamber]++;
                    z.B[off + (int)k]--;
                    if (o == O.Grab && z.B[OZ + (int)k] > z.B[OG + (int)k]) z.B[OZ + (int)k] = z.B[OG + (int)k];
                    MonsterRein(z, k);
                    Kind(aus, z);
                }
            }
        }

        private void EscapeAktivieren(Z v, List<Z> aus, int zone)
        {
            var gegner = BesteGegnerKarte(v, true);
            if (gegner < 0) return;
            for (int i = 0; i < v.B[CM]; i++)
            {
                K k = (K)v.B[OM + i];
                if (!IstUnchained(k) || (v.B[OMF + i] & FVerdeckt) != 0 || k == K.Enma || k == K.Abomination) continue;
                var s = new Schritt { Art = Art.Aktivieren, Karte = K.Escape, Ort = O.Zone, Wahlen = new[] { new Wahl(k, O.Monster), GegnerWahl(gegner) } };
                if (!Erlaubt(s)) continue;
                var z = v.Kopie(s);
                Nutze(z, E.EscapeAkt);
                ZoneRaus(z, zone); z.B[OG + (int)K.Escape]++;
                EntferneGegner(z, gegner);
                Zerstoere(z, new List<int> { i }, new List<int>(), K.Escape);
                Kind(aus, z);
            }
        }

        private void HandZerstoerer(Z v, List<Z> aus, K karte, E eff, bool nurFiendVerdeckt, O ort = O.Hand)
        {
            Ziele(v, _z, nurFiendVerdeckt);
            foreach (var t in _z.ToList())
            {
                // Platz: die zerstörte Karte macht frei, sonst muss eine Zone frei sein
                if (!t.monster && !ZoneFrei(v)) continue;
                var s = new Schritt { Art = Art.Aktivieren, Karte = karte, Ort = ort, Wahlen = new[] { WahlFeld(v, t) } };
                if (!Erlaubt(s)) continue;
                var z = v.Kopie(s);
                Nutze(z, eff);
                if (ort == O.Hand) z.B[OH + (int)karte]--;
                else { z.B[OG + (int)karte]--; if (z.B[OZ + (int)karte] > z.B[OG + (int)karte]) z.B[OZ + (int)karte] = z.B[OG + (int)karte]; }
                Zerstoere1(z, t, karte);
                MonsterRein(z, karte, ort == O.Grab ? FBoden : (byte)0);
                Kind(aus, z);
            }
        }

        private void FeldZerstoerer(Z v, List<Z> aus, K karte, E eff)
        {
            Ziele(v, _z);
            foreach (var t in _z.ToList())
            {
                if (t.monster && (K)v.B[OM + t.i] == karte) continue;       // eigener Auslöser wäre ausgeschlossen
                var s = new Schritt { Art = Art.Aktivieren, Karte = karte, Ort = O.Monster, Wahlen = new[] { WahlFeld(v, t) } };
                if (!Erlaubt(s)) continue;
                var z = v.Kopie(s);
                Nutze(z, eff);
                Zerstoere1(z, t, karte);
                Kind(aus, z);
            }
        }

        private void ShyamaFeld(Z v, List<Z> aus)
        {
            Ziele(v, _z);
            foreach (var t in _z.ToList())
            {
                var s = new Schritt { Art = Art.Aktivieren, Karte = K.Shyama, Ort = O.Monster, Wahlen = new[] { WahlFeld(v, t) } };
                if (Erlaubt(s))
                {
                    var z = v.Kopie(s);
                    Nutze(z, E.ShyamaFeld);
                    Zerstoere1(z, t, K.Shyama);
                    Kind(aus, z);
                }
                // zusätzlich 1 Zauber/Falle des Gegners
                int gz = BesteGegnerZone(v);
                if (gz >= 0)
                {
                    var s2 = new Schritt { Art = Art.Aktivieren, Karte = K.Shyama, Ort = O.Monster, Ja = true, Wahlen = new[] { WahlFeld(v, t), GegnerWahl(gz) } };
                    if (Erlaubt(s2))
                    {
                        var z = v.Kopie(s2);
                        Nutze(z, E.ShyamaFeld);
                        Zerstoere1(z, t, K.Shyama);
                        EntferneGegner(z, gz);
                        Kind(aus, z);
                    }
                }
            }
        }

        private void SaramaFeld(Z v, List<Z> aus)
        {
            foreach (K k in Monster.Concat(UnchainedZF))
            {
                if (k == K.Sarama || v.B[OG + (int)k] == 0) continue;
                bool monster = IstUnchainedMonster(k);
                if (monster ? !ZoneFrei(v) : !STFrei(v)) continue;
                var basis = v.Kopie(null);
                basis.B[OG + (int)k]--; if (basis.B[OZ + (int)k] > basis.B[OG + (int)k]) basis.B[OZ + (int)k] = basis.B[OG + (int)k];
                if (monster) MonsterRein(basis, k, FVerdeckt);
                else { basis.B[OS + basis.B[CS]] = (byte)k; basis.B[OSF + basis.B[CS]] = ZVerdeckt | ZNeu; basis.B[CS]++; }
                Ziele(basis, _z);
                foreach (var t in _z.ToList())
                {
                    var s = new Schritt { Art = Art.Aktivieren, Karte = K.Sarama, Ort = O.Monster, Wahlen = new[] { new Wahl(k, O.Grab), WahlFeld(basis, t) } };
                    if (!Erlaubt(s)) continue;
                    var z = basis.Kopie(s);
                    z.Vater = v;
                    Nutze(z, E.SaramaFeld);
                    Zerstoere1(z, t, K.Sarama);
                    Kind(aus, z);
                }
            }
        }

        private void SharaHand(Z v, List<Z> aus)
        {
            if (!ZoneFrei(v)) return;
            foreach (K k in new[] { K.Rakea, K.Aruha, K.Sarama, K.Shma, K.Shara, K.Kamura, K.Sharvara, K.Disaster, K.Abominable, K.Malice, K.Banish })
            {
                int vorrat = v.B[OH + (int)k] - (k == K.Shara ? 1 : 0);
                if (vorrat <= 0) continue;
                var basis = v.Kopie(null);
                basis.B[OH + (int)K.Shara]--; basis.B[OG + (int)K.Shara]++;
                Nutze(basis, E.SharaHand);
                basis.B[OH + (int)k]--;
                MonsterRein(basis, k);
                Ziele(basis, _z);
                foreach (var t in _z.ToList())
                {
                    var s = new Schritt { Art = Art.Aktivieren, Karte = K.Shara, Ort = O.Hand, Wahlen = new[] { new Wahl(k, O.Hand), WahlFeld(basis, t) } };
                    if (!Erlaubt(s)) continue;
                    var z = basis.Kopie(s);
                    z.Vater = v;
                    Zerstoere1(z, t, K.Shara);
                    Kind(aus, z);
                }
            }
        }

        private void KamuraHand(Z v, List<Z> aus)
        {
            Ziele(v, _z);
            var ziele = _z.ToList();
            int n = ziele.Count;
            if (n == 0) return;
            for (int a = 0; a < n; a++)
            {
                KamuraVariante(v, aus, new List<(bool, int)> { ziele[a] });
                for (int b = a + 1; b < n; b++)
                {
                    KamuraVariante(v, aus, new List<(bool, int)> { ziele[a], ziele[b] });
                    for (int c = b + 1; c < n; c++)
                        KamuraVariante(v, aus, new List<(bool, int)> { ziele[a], ziele[b], ziele[c] });
                }
            }
        }

        private static bool IstFalle(K k) => k == K.Chamber || k == K.Escape || k == K.Syncretism || k == K.Imperm || k == K.Evenly;

        private void KamuraVariante(Z v, List<Z> aus, List<(bool monster, int i)> ziele)
        {
            bool monster = ziele.Any(t => t.monster);
            bool falle = ziele.Any(t => !t.monster && IstFalle((K)v.B[OS + t.i]));
            if (!ziele.Any(t => t.monster) && !ZoneFrei(v)) return;
            var wahlen = ziele.Select(t => WahlFeld(v, t)).ToList();
            int gegner = falle ? BesteGegnerKarte(v, false) : -1;
            if (gegner >= 0) wahlen.Add(GegnerWahl(gegner));
            var s = new Schritt { Art = Art.Aktivieren, Karte = K.Kamura, Ort = O.Hand, Wahlen = wahlen.ToArray(), Ja = gegner >= 0, Ziehen = monster };
            if (!Erlaubt(s)) return;
            var z = v.Kopie(s);
            Nutze(z, E.KamuraHand);
            z.B[OH + (int)K.Kamura]--;
            Zerstoere(z, ziele.Where(t => t.monster).Select(t => t.i).ToList(), ziele.Where(t => !t.monster).Select(t => t.i).ToList(), K.Kamura);
            MonsterRein(z, K.Kamura);
            if (monster) z.B[CGez]++;
            if (gegner >= 0) EntferneGegner(z, gegner);
            Kind(aus, z);
        }

        // --- Links -----------------------------------------------------------------------------------

        private static readonly K[] LinkArten = { K.SoulLord, K.Rage, K.Anguish, K.Abominator, K.Enma, K.Abomination };

        private static bool RatingMoeglich(List<K> mat, int ziel)
        {
            int summe(int maske)
            {
                int w = 0;
                for (int i = 0; i < mat.Count; i++) w += (maske & (1 << i)) != 0 ? LinkWert(mat[i]) : 1;
                return w;
            }
            for (int m = 0; m < 1 << mat.Count; m++)
            {
                bool gueltig = true;
                for (int i = 0; i < mat.Count; i++) if ((m & (1 << i)) != 0 && !IstLink(mat[i])) gueltig = false;
                if (gueltig && summe(m) == ziel) return true;
            }
            return false;
        }

        private static bool MaterialPasst(K ziel, List<K> mat)
        {
            int wert = LinkWert(ziel);
            if (mat.Count < 2 || mat.Count > wert) return false;
            if (!RatingMoeglich(mat, wert)) return false;
            switch (ziel)
            {
                case K.SoulLord: return mat.Count == 2;
                case K.Rage: case K.Anguish: case K.Enma: return mat.Any(IstSeele);
                case K.Abomination: return mat.Any(IstLink);
                default: return true;
            }
        }

        private void LinkZuege(Z v, List<Z> aus)
        {
            int n = v.B[CM];
            var offen = new List<int>();
            int links = 0;
            for (int i = 0; i < n; i++)
            {
                if ((v.B[OMF + i] & FVerdeckt) == 0) offen.Add(i);
                if (IstLink((K)v.B[OM + i])) links++;
            }
            if (offen.Count < 2) return;
            var gesehen = new HashSet<long>();
            foreach (K ziel in LinkArten)
            {
                if (v.B[OD + (int)ziel] == 0) continue;
                for (int maske = 1; maske < 1 << offen.Count; maske++)
                {
                    int bits = 0;
                    for (int b = 0; b < offen.Count; b++) if ((maske & (1 << b)) != 0) bits++;
                    if (bits < 2 || bits > 4) continue;
                    var idx = new List<int>();
                    for (int b = 0; b < offen.Count; b++) if ((maske & (1 << b)) != 0) idx.Add(offen[b]);
                    var mat = idx.Select(i => (K)v.B[OM + i]).ToList();
                    if (links > 0 && links != mat.Count(IstLink)) continue;      // jeder Link auf dem Feld muss mit, sonst fehlt die Zone
                    if (!MaterialPasst(ziel, mat)) continue;
                    long key = (long)ziel;
                    foreach (int i in idx.OrderBy(i => (v.B[OM + i] * 8 + v.B[OMF + i]))) key = key * 251 + v.B[OM + i] * 8 + v.B[OMF + i];
                    if (!gesehen.Add(key)) continue;
                    var s = new Schritt { Art = Art.Link, Karte = ziel, Ort = O.Extra, Wahlen = mat.Select(k => new Wahl(k, O.Monster)).ToArray() };
                    if (!Erlaubt(s)) continue;
                    var z = v.Kopie(s);
                    var flags = idx.Select(i => z.B[OMF + i]).ToList();
                    foreach (int i in idx.OrderByDescending(i => i)) MonsterRaus(z, i);
                    for (int j = 0; j < mat.Count; j++)
                    {
                        if ((flags[j] & FBoden) != 0) z.B[OD + (int)mat[j]]++;
                        else ZumFriedhof(z, mat[j], false);
                    }
                    z.B[OD + (int)ziel]--;
                    NachLink(z, ziel);
                    Kind(aus, z);
                }
            }
        }

        private void NachLink(Z z, K ziel)
        {
            MonsterRein(z, ziel);
            if (ziel == K.SoulLord && Frei(z, E.SoulSuche)) Neu(z, T.SoulSuche);
            if (Frei(z, E.WailingLink) && ZoneMit(z, K.Wailing, false)) Neu(z, T.WailingLink);
        }

        private void GegnerLink(Z v, List<Z> aus, int i, K quelle, E eff)
        {
            var gm = GegnerMonster;
            for (int g = 0; g < gm.Count; g++)
            {
                if ((v.B[CGegM] & (1 << g)) == 0 || !gm[g].Offen || gm[g].Immun) continue;
                int gi = _gegner.IndexOf(gm[g]);
                foreach (K ziel in new[] { K.Enma, K.Abomination, K.Abominator, K.Anguish, K.Rage })
                {
                    if (ziel == quelle || v.B[OD + (int)ziel] == 0) continue;
                    int mein = LinkWert(quelle), wert = LinkWert(ziel), sein = Math.Max(1, gm[g].Link);
                    bool passt = 1 + 1 == wert || mein + 1 == wert || 1 + sein == wert || mein + sein == wert;
                    if (!passt) continue;
                    if (ziel == K.Abomination && !IstLink(quelle) && gm[g].Link == 0) continue;
                    if (ziel == K.Rage && quelle == K.Rage) continue;
                    var s = new Schritt { Art = Art.Aktivieren, Karte = quelle, Ort = O.Monster, Wahlen = new[] { GegnerWahl(gi), new Wahl(ziel, O.Extra) } };
                    if (!Erlaubt(s)) continue;
                    var z = v.Kopie(s);
                    Nutze(z, eff);
                    byte fl = z.B[OMF + i];
                    MonsterRaus(z, i);
                    if ((fl & FBoden) != 0) z.B[OD + (int)quelle]++; else ZumFriedhof(z, quelle, false);
                    EntferneGegner(z, gi);
                    z.B[OD + (int)ziel]--;
                    NachLink(z, ziel);
                    Kind(aus, z);
                }
            }
        }

        // --- Gegnerkarten ---------------------------------------------------------------------------

        private int BesteGegnerKarte(Z z, bool ziel)
        {
            int best = -1, wert = -1;
            var gm = GegnerMonster; var gz = GegnerZonen;
            for (int i = 0; i < gm.Count; i++)
                if ((z.B[CGegM] & (1 << i)) != 0 && !gm[i].Immun && !gm[i].Unzerst && gm[i].Wert > wert) { wert = gm[i].Wert; best = _gegner.IndexOf(gm[i]); }
            for (int i = 0; i < gz.Count; i++)
                if ((z.B[CGegZ] & (1 << i)) != 0 && !gz[i].Unzerst && gz[i].Wert > wert) { wert = gz[i].Wert; best = _gegner.IndexOf(gz[i]); }
            return best;
        }
        private int BesteGegnerZone(Z z)
        {
            int best = -1, wert = -1;
            var gz = GegnerZonen;
            for (int i = 0; i < gz.Count; i++)
                if ((z.B[CGegZ] & (1 << i)) != 0 && !gz[i].Unzerst && gz[i].Wert > wert) { wert = gz[i].Wert; best = _gegner.IndexOf(gz[i]); }
            return best;
        }
        private Wahl GegnerWahl(int i) => new Wahl(K.Anderes, _gegner[i].Monster ? O.GegnerM : O.GegnerZ, false, i);
        private void EntferneGegner(Z z, int i)
        {
            var g = _gegner[i];
            int pos = g.Monster ? GegnerMonster.IndexOf(g) : GegnerZonen.IndexOf(g);
            if (g.Monster) z.B[CGegM] = (byte)(z.B[CGegM] & ~(1 << pos));
            else z.B[CGegZ] = (byte)(z.B[CGegZ] & ~(1 << pos));
        }

        // =====================================================================================
        // Auslöser auflösen
        // =====================================================================================

        private void Weg(Z z, int index)
        {
            int last = --z.B[CP];
            for (int j = index; j < last; j++) z.B[OP + j] = z.B[OP + j + 1];
        }

        private void Ausloeser(Z v, List<Z> aus)
        {
            int gruppe = v.Gruppe;
            for (int i = 0; i < v.B[CP]; i++)
            {
                T t = (T)v.B[OP + i];
                bool doppelt = false;
                for (int j = 0; j < i; j++) if (v.B[OP + j] == (byte)t) doppelt = true;
                if (doppelt) continue;
                Aufloesen(v, aus, i, t, gruppe);
                // Auslassen: der Auslöser verfällt
                var skip = new Schritt { Art = Art.Auslassen, Karte = KarteVon(t), Ort = OrtVon(t), Gruppe = gruppe };
                var z = v.Kopie(skip);
                Weg(z, i);
                Kind(aus, z);
            }
        }

        private static K KarteVon(T t)
        {
            switch (t)
            {
                case T.ShmaNS: case T.ShmaZ: return K.Shma;
                case T.AruhaZ: return K.Aruha;
                case T.SaramaZ: return K.Sarama;
                case T.RakeaZ: return K.Rakea;
                case T.PrisonZ: return K.Prison;
                case T.WailingZ: case T.WailingLink: return K.Wailing;
                case T.ChamberZ: return K.Chamber;
                case T.EscapeZ: return K.Escape;
                case T.SyncZ: return K.Syncretism;
                case T.KamuraBombe: return K.Kamura;
                case T.DisasterZ: return K.Disaster;
                case T.SharvaraGrab: return K.Sharvara;
                case T.RageZ: return K.Rage;
                case T.AnguishZ: return K.Anguish;
                case T.SharaGrab: return K.Shara;
                case T.SoulGrab: case T.SoulSuche: return K.SoulLord;
                case T.AbomHand: case T.AbomPop: return K.Abominable;
                case T.MaliceAdd: return K.Malice;
                case T.BanishAdd: return K.Banish;
                default: return K.Abomination;
            }
        }
        private static O OrtVon(T t)
        {
            switch (t)
            {
                case T.ShmaNS: case T.SoulSuche: case T.AbomPop: case T.AbominationZ: return O.Monster;
                case T.WailingLink: return O.Zone;
                case T.AbomHand: return O.Hand;
                default: return O.Grab;
            }
        }

        private void Aufloesen(Z v, List<Z> aus, int i, T t, int gruppe)
        {
            K quelle = KarteVon(t);
            O ort = OrtVon(t);
            Schritt S(params Wahl[] w) => new Schritt { Art = Art.Ausloeser, Karte = quelle, Ort = ort, Gruppe = gruppe, Wahlen = w };
            switch (t)
            {
                case T.ShmaNS:
                    if (!ZoneFrei(v)) return;
                    foreach (K k in new[] { K.Rakea, K.Aruha, K.Sarama, K.Shara })
                    {
                        if (v.B[OD + (int)k] == 0) continue;
                        var basis = v.Kopie(null);
                        Weg(basis, i);
                        Nutze(basis, E.ShmaNS);
                        basis.B[OD + (int)k]--;
                        MonsterRein(basis, k);
                        Ziele(basis, _z);
                        foreach (var ziel in _z.ToList())
                        {
                            var z = basis.Kopie(S(new Wahl(k, O.Deck), WahlFeld(basis, ziel)));
                            z.Vater = v;
                            Zerstoere1(z, ziel, K.Shma);
                            Kind(aus, z);
                        }
                    }
                    return;
                case T.ShmaZ: BeschwoereUnchained(v, aus, i, E.ShmaZ, K.Shma, S); return;
                case T.AruhaZ: BeschwoereUnchained(v, aus, i, E.AruhaZ, K.Aruha, S); return;
                case T.SaramaZ: BeschwoereUnchained(v, aus, i, E.SaramaZ, K.Sarama, S); return;
                case T.RakeaZ: BeschwoereUnchained(v, aus, i, E.RakeaZ, K.Rakea, S); return;
                case T.PrisonZ: BeschwoereUnchained(v, aus, i, E.PrisonZ, K.N, S, true); return;
                case T.WailingZ: BeschwoereUnchained(v, aus, i, E.WailingZ, K.N, S, true); return;
                case T.ChamberZ: BeschwoereUnchained(v, aus, i, E.ChamberZ, K.N, S, true); return;
                case T.EscapeZ: BeschwoereUnchained(v, aus, i, E.EscapeZ, K.N, S, true); return;
                case T.SyncZ: BeschwoereUnchained(v, aus, i, E.SyncZ, K.N, S, true); return;
                case T.KamuraBombe: Bombe(v, aus, i, S); return;
                case T.DisasterZ:
                    if (!ZoneFrei(v)) return;
                    foreach (K k in Monster)
                    {
                        if (k == K.Disaster || v.B[OG + (int)k] == 0) continue;
                        var z = v.Kopie(S(new Wahl(k, O.Grab)));
                        Weg(z, i); Nutze(z, E.DisasterZ);
                        z.B[OG + (int)k]--; if (z.B[OZ + (int)k] > z.B[OG + (int)k]) z.B[OZ + (int)k] = z.B[OG + (int)k];
                        MonsterRein(z, k);
                        Kind(aus, z);
                    }
                    return;
                case T.SharvaraGrab:
                    foreach (K k in UnchainedZF)
                    {
                        if (v.B[OD + (int)k] == 0 || !STFrei(v)) continue;
                        var z = v.Kopie(S(new Wahl(k, O.Deck)));
                        Weg(z, i); Nutze(z, E.SharvaraGrab);
                        z.B[OD + (int)k]--;
                        // Der Permanentzauber lässt sich im selben Zug aktivieren, Fallen erst im nächsten
                        z.B[OS + z.B[CS]] = (byte)k; z.B[OSF + z.B[CS]] = ZVerdeckt | ZNeu; z.B[CS]++;
                        Kind(aus, z);
                    }
                    return;
                case T.RageZ: GrabZurHand(v, aus, i, E.RageZ, K.Rage, S); return;
                case T.AnguishZ: GrabZurHand(v, aus, i, E.AnguishZ, K.Anguish, S); return;
                case T.SharaGrab:
                    if (v.B[OG + (int)K.Shara] == 0) return;
                    {
                        var z = v.Kopie(S()); Weg(z, i); Nutze(z, E.SharaGrab);
                        z.B[OG + (int)K.Shara]--; if (z.B[OZ + (int)K.Shara] > z.B[OG + (int)K.Shara]) z.B[OZ + (int)K.Shara] = z.B[OG + (int)K.Shara];
                        z.B[OH + (int)K.Shara]++;
                        Kind(aus, z);
                        if (ZoneFrei(v))
                        {
                            var sp = S(); sp.Beschwoeren = true;
                            var y = v.Kopie(sp); Weg(y, i); Nutze(y, E.SharaGrab);
                            y.B[OG + (int)K.Shara]--; if (y.B[OZ + (int)K.Shara] > y.B[OG + (int)K.Shara]) y.B[OZ + (int)K.Shara] = y.B[OG + (int)K.Shara];
                            MonsterRein(y, K.Shara, FBoden);
                            Kind(aus, y);
                        }
                    }
                    return;
                case T.SoulGrab: SoulGrab(v, aus, i, S); return;
                case T.AbomHand:
                    if (!ZoneFrei(v) || v.B[OH + (int)K.Abominable] == 0) return;
                    {
                        var z = v.Kopie(S()); Weg(z, i); Nutze(z, E.AbomSS);
                        z.B[OH + (int)K.Abominable]--;
                        MonsterRein(z, K.Abominable);
                        if (Frei(z, E.AbomPop)) Neu(z, T.AbomPop);
                        Kind(aus, z);
                    }
                    return;
                case T.AbomPop: AbominablePop(v, aus, i, S); return;
                case T.SoulSuche:
                    foreach (K k in Monster)
                    {
                        if (v.B[OD + (int)k] > 0)
                        {
                            var z = v.Kopie(S(new Wahl(k, O.Deck))); Weg(z, i); Nutze(z, E.SoulSuche);
                            z.B[OD + (int)k]--; z.B[OH + (int)k]++;
                            Kind(aus, z);
                        }
                        else if (v.B[OG + (int)k] > 0)
                        {
                            var z = v.Kopie(S(new Wahl(k, O.Grab))); Weg(z, i); Nutze(z, E.SoulSuche);
                            z.B[OG + (int)k]--; if (z.B[OZ + (int)k] > z.B[OG + (int)k]) z.B[OZ + (int)k] = z.B[OG + (int)k];
                            z.B[OH + (int)k]++;
                            Kind(aus, z);
                        }
                    }
                    return;
                case T.WailingLink: EinZiel(v, aus, i, E.WailingLink, K.Wailing, S); return;
                case T.AbominationZ: EinZiel(v, aus, i, E.AbominationZ, K.Abomination, S); return;
                case T.MaliceAdd: Dunkel(v, aus, i, K.Malice, E.MaliceAdd, S); return;
                case T.BanishAdd: Dunkel(v, aus, i, K.Banish, E.BanishAdd, S); return;
            }
        }

        private void Dunkel(Z v, List<Z> aus, int i, K k, E e, Func<Wahl[], Schritt> S)
        {
            if (v.B[OG + (int)k] == 0) return;
            var z = v.Kopie(S(Array.Empty<Wahl>())); Weg(z, i); Nutze(z, e);
            z.B[OG + (int)k]--; z.B[OH + (int)k]++;
            Kind(aus, z);
        }

        private void BeschwoereUnchained(Z v, List<Z> aus, int i, E eff, K ausser, Func<Wahl[], Schritt> S, bool nurDeck = false)
        {
            if (!ZoneFrei(v)) return;
            foreach (K k in Monster)
            {
                if (k == ausser) continue;
                var z = v.Kopie(null);
                if (!HoleHandDeck(z, k, out O ort, nurDeck)) continue;
                z.Schritt = S(new[] { new Wahl(k, ort) });
                Weg(z, i); Nutze(z, eff);
                MonsterRein(z, k);
                if (k == K.Abominable && Frei(z, E.AbomPop)) { Nutze(z, E.AbomSS); Neu(z, T.AbomPop); }
                Kind(aus, z);
            }
        }

        private void GrabZurHand(Z v, List<Z> aus, int i, E eff, K ausser, Func<Wahl[], Schritt> S)
        {
            foreach (K k in Monster.Concat(new[] { K.Malice, K.Banish }))
            {
                if (k == ausser || v.B[OG + (int)k] == 0) continue;
                var z = v.Kopie(S(new[] { new Wahl(k, O.Grab) })); Weg(z, i); Nutze(z, eff);
                z.B[OG + (int)k]--; if (z.B[OZ + (int)k] > z.B[OG + (int)k]) z.B[OZ + (int)k] = z.B[OG + (int)k];
                z.B[OH + (int)k]++;
                Kind(aus, z);
            }
        }

        private void SoulGrab(Z v, List<Z> aus, int i, Func<Wahl[], Schritt> S)
        {
            if (!ZoneFrei(v)) return;
            foreach (K k in Monster.Concat(new[] { K.Malice, K.Banish, K.Rage, K.Anguish, K.Abominator }))
            {
                foreach (O o in new[] { O.Grab, O.Hand })
                {
                    int off = o == O.Grab ? OG : OH;
                    if (v.B[off + (int)k] == 0 || o == O.Hand && IstLink(k)) continue;
                    var basis = v.Kopie(null);
                    Weg(basis, i); Nutze(basis, E.SoulGrab);
                    basis.B[OG + (int)K.SoulLord]--;
                    if (basis.B[OZ + (int)K.SoulLord] > basis.B[OG + (int)K.SoulLord]) basis.B[OZ + (int)K.SoulLord] = basis.B[OG + (int)K.SoulLord];
                    basis.B[off + (int)k]--;
                    if (o == O.Grab && basis.B[OZ + (int)k] > basis.B[OG + (int)k]) basis.B[OZ + (int)k] = basis.B[OG + (int)k];
                    MonsterRein(basis, k);
                    var ohne = basis.Kopie(S(new[] { new Wahl(k, o) }));
                    ohne.Vater = v;
                    Kind(aus, ohne);
                    Ziele(basis, _z);
                    foreach (var ziel in _z.ToList())
                    {
                        var st = S(new[] { new Wahl(k, o), WahlFeld(basis, ziel) }); st.Ja = true;
                        var z = basis.Kopie(st);
                        z.Vater = v;
                        Zerstoere1(z, ziel, K.SoulLord);
                        Kind(aus, z);
                    }
                }
            }
        }

        private void AbominablePop(Z v, List<Z> aus, int i, Func<Wahl[], Schritt> S)
        {
            // Kosten: 1 Karte abwerfen. Gegnerkarte zerstören, sonst eine eigene mit freiem Auslöser.
            var hand = Enumerable.Range(0, NK).Where(k => v.B[OH + k] > 0).Select(k => (K)k).ToList();
            if (hand.Count == 0) return;
            foreach (K ab in hand)
            {
                int g = BesteGegnerKarte(v, false);
                if (g >= 0)
                {
                    var z = v.Kopie(S(new[] { new Wahl(ab, O.Hand), GegnerWahl(g) })); Weg(z, i); Nutze(z, E.AbomPop);
                    z.B[OH + (int)ab]--; ZumFriedhof(z, ab, false);
                    EntferneGegner(z, g);
                    Kind(aus, z);
                }
                Ziele(v, _z);
                foreach (var ziel in _z.ToList())
                {
                    var z = v.Kopie(S(new[] { new Wahl(ab, O.Hand), WahlFeld(v, ziel) })); Weg(z, i); Nutze(z, E.AbomPop);
                    z.B[OH + (int)ab]--; ZumFriedhof(z, ab, false);
                    Zerstoere1(z, ziel, K.Abominable);
                    Kind(aus, z);
                }
            }
        }

        private void EinZiel(Z v, List<Z> aus, int i, E eff, K quelle, Func<Wahl[], Schritt> S)
        {
            int g = BesteGegnerKarte(v, true);
            if (g >= 0)
            {
                var z = v.Kopie(S(new[] { GegnerWahl(g) })); Weg(z, i); Nutze(z, eff);
                EntferneGegner(z, g);
                Kind(aus, z);
            }
            Ziele(v, _z);
            foreach (var ziel in _z.ToList())
            {
                var z = v.Kopie(S(new[] { WahlFeld(v, ziel) })); Weg(z, i); Nutze(z, eff);
                Zerstoere1(z, ziel, quelle);
                Kind(aus, z);
            }
        }

        private void Bombe(Z v, List<Z> aus, int i, Func<Wahl[], Schritt> S)
        {
            var z = v.Kopie(S(Array.Empty<Wahl>())); Weg(z, i); Nutze(z, E.KamuraZ);
            var mon = new List<int>();
            for (int m = 0; m < z.B[CM]; m++)
            {
                K k = (K)z.B[OM + m];
                if (k == K.Enma && Frei(z, E.EnmaErsatz)) { Nutze(z, E.EnmaErsatz); continue; }  // Enma bleibt, Ersatz zerstört eine offene Karte
                mon.Add(m);
            }
            var gm = GegnerMonster;
            for (int g = 0; g < gm.Count; g++)
                if ((z.B[CGegM] & (1 << g)) != 0 && !gm[g].Immun && !gm[g].Unzerst) z.B[CGegM] = (byte)(z.B[CGegM] & ~(1 << g));
            Zerstoere(z, mon, new List<int>(), K.Kamura);
            Kind(aus, z);
        }

        // =====================================================================================
        // Bewertung
        // =====================================================================================

        private static int WertFeld(K k)
        {
            switch (k)
            {
                case K.Enma: return 50;
                case K.Abomination: return 36;
                case K.Abominator: return 20;
                case K.Anguish: return 16;
                case K.Rage: return 14;
                case K.SoulLord: return 10;
                case K.Rakea: return 13;
                case K.Kamura: return 12;
                case K.Abominable: return 12;
                case K.Sarama: return 8;
                case K.Aruha: return 8;
                case K.Disaster: return 9;
                case K.Shyama: return 7;
                case K.Sharvara: return 7;
                case K.Shma: return 7;
                case K.Shara: return 6;
                case K.Malice: return 8;
                case K.Banish: return 5;
                default: return 3;
            }
        }
        private static int WertHand(K k)
        {
            switch (k)
            {
                case K.Imperm: return 13;
#if !OHNE_HANDBOARD
                // Karten auf der Hand halten (Pool 02.11.: je 480 Duelle 51,0 gegen 49,4 %, also kein gesicherter Unterschied)
                case K.Sharvara: return 20;
                case K.Shara: return 18;
#else
                case K.Sharvara: return 12;
                case K.Shara: return 11;
#endif
                case K.Malice: return 8;
                case K.Banish: return 5;
                case K.Prison: return 9;
                case K.Shma: return 8;
                case K.Kamura: return 9;
                case K.Aruha: return 7;
                case K.Reinc: return 7;
                case K.Rakea: return 7;
                case K.Sarama: return 6;
                case K.Evenly: return 5;
                case K.Abominable: return 8;
                case K.Disaster: return 5;
                case K.Shyama: return 6;
                case K.Chamber: return 8;
                case K.Escape: return 7;
                case K.Syncretism: return 6;
                case K.Wailing: return 5;
                default: return 4;
            }
        }
        private static int WertZone(K k, bool verdeckt)
        {
            switch (k)
            {
                case K.Imperm: return 13;
                case K.Chamber: return 10;
                case K.Escape: return 10;
                case K.Syncretism: return 8;
                case K.Wailing: return verdeckt ? 6 : 5;
                case K.Prison: return 6;
                case K.Reinc: return verdeckt ? 3 : 4;
                case K.Evenly: return 5;
                default: return 2;
            }
        }

        /// <summary>Wert des Zustands, wenn der Zug jetzt enden würde (mit der End Phase von Enma).</summary>
        internal int Bewerte(Z z)
        {
            int w = 0;
            bool enma = false, link4 = false;
            var mon = new List<K>();
            int atk = 0;
            for (int i = 0; i < z.B[CM]; i++)
            {
                K k = (K)z.B[OM + i];
                bool verdeckt = (z.B[OMF + i] & FVerdeckt) != 0;
                mon.Add(k);
                w += verdeckt ? 3 : WertFeld(k);
                if (k == K.Enma) enma = true;
                if ((k == K.Enma || k == K.Abomination) && !verdeckt) link4 = true;
                if (!verdeckt) atk += Atk(k);
            }
            for (int i = 0; i < z.B[CS]; i++) w += WertZone((K)z.B[OS + i], (z.B[OSF + i] & ZVerdeckt) != 0);
            for (int k = 0; k < NK; k++) if (z.B[OH + k] > 0) w += z.B[OH + k] * WertHand((K)k);
            w += z.B[CGez] * 8;
            // Friedhof: Rage Abominator stört nur mit Link-4 auf dem Feld, Soul Lord und Shara lösen im Gegnerzug aus
            if (z.B[OG + (int)K.Abominator] > 0) w += link4 ? 24 : 4;
            if (z.B[OG + (int)K.SoulLord] > 0) w += 6;
            if (z.B[OG + (int)K.Shara] > 0) w += 4;
            if (z.B[OG + (int)K.Shyama] > 0) w += 3;
            if (z.B[OG + (int)K.Malice] > 0) w += 1;
            // Enma belebt in der End Phase zwei zerstörte Monster
            if (enma && Frei(z, E.EnmaEnde) && !_hauptphase2)
            {
                var best = new List<int>();
                foreach (K k in Monster.Concat(new[] { K.Malice, K.Banish, K.Rage, K.Anguish, K.SoulLord }))
                    for (int n = 0; n < Math.Min((int)z.B[OZ + (int)k], (int)z.B[OG + (int)k]); n++) best.Add(WertFeld(k));
                int frei = Math.Max(0, 5 - MainBelegt(z));
                w += best.OrderByDescending(x => x).Take(Math.Min(2, frei)).Sum();
            }
            if (_gegnerzug && enma && !Frei(z, E.EnmaErsatz)) w -= 15;    // Enma steht den Rest des Zuges ohne Schutz da
            // Gegner
            var gm = GegnerMonster; var gz = GegnerZonen;
            int offeneDef = 0, restAtk = atk;
            for (int i = 0; i < gm.Count; i++)
            {
                bool da = (z.B[CGegM] & (1 << i)) != 0;
                if (!da) w += gm[i].Wert;
                else offeneDef++;
            }
            for (int i = 0; i < gz.Count; i++)
                if ((z.B[CGegZ] & (1 << i)) == 0) w += gz[i].Wert;
            // Schaden: im zweiten Zug zählt, was durchkommt
            if (!_ersterZug && !_hauptphase2)
            {
                int blocker = 0;
                for (int i = 0; i < gm.Count; i++) if ((z.B[CGegM] & (1 << i)) != 0) blocker++;
                var ang = z.B.Skip(OM).Take(z.B[CM]).Select((b, i) => (K)b).Select(Atk).OrderByDescending(a => a).ToList();
                int dmg = ang.Skip(blocker).Sum();
                w += Math.Min(dmg, _gegnerLP) / 100;
                if (dmg >= _gegnerLP) w += 200;
            }
            return w;
        }

        private static int Atk(K k)
        {
            switch (k)
            {
                case K.Enma: case K.Abomination: case K.Abominable: return 3000;
                case K.Abominator: return 2100;
                case K.Anguish: return 2400;
                case K.Rage: return 1800;
                case K.SoulLord: return 2000;
                case K.Kamura: case K.Sharvara: case K.Shyama: return 2000;
                case K.Malice: case K.Banish: return 1600;
                case K.Disaster: return 1500;
                default: return 1500;
            }
        }

        /// <summary>Bewertung für die Strahlsuche: Endwert plus Guthaben für noch freie Effekte, sonst fliegen Zwischenzustände zu früh raus.</summary>
        private int Rang(Z z)
        {
            int w = Bewerte(z);
            foreach (E e in FreieMotor) if (Frei(z, e)) w += 2;
            for (int i = 0; i < z.B[CP]; i++) w += 8;
            for (int i = 0; i < z.B[CQ]; i++) w += 8;
            w += z.B[OD + (int)K.Enma] == 0 ? 6 : 0;
            return w;
        }
        private static readonly E[] FreieMotor = { E.ShmaZ, E.AruhaZ, E.SaramaZ, E.RakeaZ, E.SharaGrab, E.SoulGrab, E.SharvaraGrab, E.ShyamaFeld, E.ShyamaGrab, E.ChamberZ, E.SoulSuche };

        // =====================================================================================
        // Suche
        // =====================================================================================

        /// <summary>Strahlsuche über Zugfolgen. Liefert den besten Plan (Schritte in Reihenfolge) und seinen Wert.</summary>
        internal List<Z> Suche(Z start, int breite, int millis, out int wert)
        {
            var uhr = Stopwatch.StartNew();
            var gesehen = new HashSet<ulong>();
            var schicht = new List<Z> { start };
            Z best = start;
            int bestWert = start.B[CP] > 0 ? int.MinValue : Bewerte(start);
            var kinder = new List<Z>();
            for (int tiefe = 0; tiefe < 70 && schicht.Count > 0; tiefe++)
            {
                kinder.Clear();
                foreach (Z v in schicht)
                {
                    int vor = kinder.Count;
                    Erweitere(v, kinder);
                    _knoten += kinder.Count - vor;
                    if (uhr.ElapsedMilliseconds > millis) break;
                }
                var neu = new List<(int rang, Z z)>(kinder.Count);
                foreach (Z k in kinder)
                {
                    if (!gesehen.Add(k.Hash())) continue;
                    if (k.B[CP] == 0)
                    {
                        int w = Bewerte(k);
                        if (w > bestWert) { bestWert = w; best = k; }
                    }
                    neu.Add((Rang(k), k));
                }
                if (uhr.ElapsedMilliseconds > millis) break;
                neu.Sort((a, b) => b.rang.CompareTo(a.rang));
                schicht = neu.Take(breite).Select(x => x.z).ToList();
            }
            wert = bestWert;
            var pfad = new List<Z>();
            for (Z z = best; z != null && z.Schritt != null; z = z.Vater) pfad.Add(z);
            pfad.Reverse();
            return pfad;
        }
    }
}
