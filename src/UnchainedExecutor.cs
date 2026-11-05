using System;
using System.Collections.Generic;
using System.Linq;
using YGOSharp.OCGWrapper;
using YGOSharp.OCGWrapper.Enums;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    /// <summary>
    /// KI für Ketos Deck „Unchained“ .
    /// Eigener Zug: Der Zugplaner (UnchainedPlaner.cs) sucht vom echten Zustand aus die beste Zugfolge, Ziel als Erster Enma Lord Yama
    /// mit Rage Abominator im Friedhof, als Zweiter zusätzlich Gegnerkarten abräumen. Stimmt das Spiel nicht mehr mit dem Plan
    /// überein (Handfalle, gezogene Karte), wird neu geplant.
    /// Gegnerzug: feste Regeln für die Störungen (Rage Abominator, Impermanence, Rage, Escape, Rakea, Kamura-Bombe, Enmas Schutz).
    /// </summary>
    [Deck("UnchainedAI", "AI_UnchainedAI")]
    public class UnchainedAIExecutor : DefaultExecutor
    {
        // --- Main Deck ---------------------------------------------------------------------------
        private const int Shma = 31531914, Shara = 95136979, Kamura = 68625623, Aruha = 26236560, Sarama = 31588572, Rakea = 53624265,
            Sharvara = 41165831, Shyama = 88554436, Disaster = 89019964, Abominable = 1966438, Malice = 87988305, Banish = 51993760,
            Prison = 27412542, Reinc = 67803035, Wailing = 54807656, Chamber = 80801743, Escape = 53417695, Syncretism = 93898740,
            Imperm = 10045474, Evenly = 15693423;
        // --- Extra Deck ---------------------------------------------------------------------------
        private const int SoulLord = 24269961, Rage = 67680512, Anguish = 93084621, Abominator = 21419436, Enma = 94014327,
            Abomination = 29479266, AbominationAlt = 29479265;
        /// <summary>Unbeeinflussbar: Lunalight Liger Dancer (Disaster, Anguish, Rage und Kamura-Bombe verpuffen).</summary>
        private const int Liger = 54701958;

        private static readonly Dictionary<int, K> Arten = new Dictionary<int, K>
        {
            { Shma, K.Shma }, { Shara, K.Shara }, { Kamura, K.Kamura }, { Aruha, K.Aruha }, { Sarama, K.Sarama }, { Rakea, K.Rakea },
            { Sharvara, K.Sharvara }, { Shyama, K.Shyama }, { Disaster, K.Disaster }, { Abominable, K.Abominable }, { Malice, K.Malice },
            { Banish, K.Banish }, { Prison, K.Prison }, { Reinc, K.Reinc }, { Wailing, K.Wailing }, { Chamber, K.Chamber }, { Escape, K.Escape },
            { Syncretism, K.Syncretism }, { Imperm, K.Imperm }, { Evenly, K.Evenly }, { SoulLord, K.SoulLord }, { Rage, K.Rage },
            { Anguish, K.Anguish }, { Abominator, K.Abominator }, { Enma, K.Enma }, { Abomination, K.Abomination }, { AbominationAlt, K.Abomination }
        };

        /// <summary>Ketos Main Deck „Unchained“ (Stand 2026-11-02). Bei jeder Deckänderung mitändern.</summary>
        private static readonly Dictionary<K, int> Copies = new Dictionary<K, int>
        {
            { K.Abominable, 1 }, { K.Kamura, 3 }, { K.Disaster, 1 }, { K.Sharvara, 2 }, { K.Shyama, 1 }, { K.Shma, 3 }, { K.Shara, 3 },
            { K.Banish, 1 }, { K.Malice, 1 }, { K.Aruha, 3 }, { K.Sarama, 3 }, { K.Rakea, 2 }, { K.Prison, 3 }, { K.Wailing, 1 },
            { K.Reinc, 3 }, { K.Imperm, 3 }, { K.Evenly, 2 }, { K.Escape, 2 }, { K.Chamber, 1 }, { K.Syncretism, 1 }
        };

        /// <summary>Gegnerische Negierer und Bosse, die zuerst weg müssen (Swordsoul Grandmaster - Chixiao, Adamancipator Risen - Dragite,
        /// Baronne de Fleur, Apollousa, Accesscode Talker, Liger Dancer als Wert, S:P Little Knight).</summary>
        private static readonly int[] GegnerStoerer = { 69248256, 9464441, 84815190, 4280258, 86066372, 29301450 };

        // --- Plan ----------------------------------------------------------------------------------
        private List<Schritt> _plan = new List<Schritt>();
        private List<Planer.Z> _planZ = new List<Planer.Z>();
        private int _pos;
        private readonly HashSet<int> _gruppeAktiv = new HashSet<int>();  // aktivierte Auslöser-Schritte der laufenden Kette
        private readonly List<Wahl> _queue = new List<Wahl>();
        private readonly HashSet<K> _ja = new HashSet<K>();
        private bool _sharaBeschwoeren;
        private bool _neuPlanen = true;
        private int _planZahl;
        private readonly HashSet<string> _verboten = new HashSet<string>();
        private List<GegnerKarte> _gegner = new List<GegnerKarte>();
        private List<ClientCard> _gegnerRef = new List<ClientCard>();
        private bool _imIdle;
        private IList<ClientCard> _angeboten;
        private ClientCard _impZiel;
        private ClientCard _ziel;

        // --- Merkliste des Zuges --------------------------------------------------------------------
        private readonly HashSet<Planer.E> _used = new HashSet<Planer.E>();
        private readonly List<ClientCard> _zerstoert = new List<ClientCard>();       // diesen Zug durch Zerstörung vom Feld in den Friedhof
        private readonly List<K> _matErwartet = new List<K>();
        private readonly HashSet<ClientCard> _neuGesetzt = new HashSet<ClientCard>();
        private readonly HashSet<ClientCard> _boden = new HashSet<ClientCard>();     // „unter das Deck, wenn es das Feld verlässt“
        private bool _nsUsed;
        private int _aktivId;

        public UnchainedAIExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Erste Einträge: laufen bei jeder Hauptphasen-Abfrage zuerst
            base.AddExecutor(ExecutorType.GoToEndPhase, IdleHook);
            base.AddExecutor(ExecutorType.GoToBattlePhase, ErstBattlePhase);

            // Zug-Plan
            AddExecutor(ExecutorType.Activate, PlanAktivieren);
            AddExecutor(ExecutorType.Summon, PlanNormal);
            AddExecutor(ExecutorType.SpSummon, PlanLink);
            AddExecutor(ExecutorType.SpellSet, PlanSetzen);

            // Störungen und Antworten
            AddExecutor(ExecutorType.Activate, Enma, EnmaEffekt);
            AddExecutor(ExecutorType.Activate, Abominator, AbominatorEffekt);
            AddExecutor(ExecutorType.Activate, Imperm, ImpermEffekt);
            AddExecutor(ExecutorType.Activate, Evenly, EvenlyEffekt);
            AddExecutor(ExecutorType.Activate, Malice, MaliceEffekt);
#if !OHNE_BANISH
            // Blockt einen Angriff mit einer Stufe-8 aus dem Friedhof (Ketos Rokket-Duell 02.11.: bei 300 LP auf der Hand vergessen)
            AddExecutor(ExecutorType.Activate, Banish, () => !PlanLehntAb() && (Card.Location == CardLocation.Grave || Bot.Graveyard.Any(IstStufe8)));
#endif
            AddExecutor(ExecutorType.Activate, Rage, RageEffekt);
            AddExecutor(ExecutorType.Activate, Escape, EscapeEffekt);
            AddExecutor(ExecutorType.Activate, Syncretism, SyncretismEffekt);
            AddExecutor(ExecutorType.Activate, Chamber, ChamberEffekt);
            AddExecutor(ExecutorType.Activate, Rakea, RakeaEffekt);
            AddExecutor(ExecutorType.Activate, Sharvara, SharvaraEffekt);
            AddExecutor(ExecutorType.Activate, Shara, SharaEffekt);
            AddExecutor(ExecutorType.Activate, Kamura, KamuraBombeEffekt);
            AddExecutor(ExecutorType.Activate, RegelAusloeser);

            // Am Ende des Zuges
            AddExecutor(ExecutorType.SpellSet, RestSetzen);
            AddExecutor(ExecutorType.Repos, MonsterRepos);
        }

        // =====================================================================================
        // Absicherung und Protokoll (WindBot\UnchainedAI.log)
        // =====================================================================================

        private static readonly string LogPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UnchainedAI.log");

        private void Log(string text)
        {
            try
            {
                var info = new System.IO.FileInfo(LogPath);
                if (info.Exists && info.Length > 2_000_000) info.Delete();
                System.IO.File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss} Z{Duel.Turn} {Duel.Phase} {text}{Environment.NewLine}");
            }
            catch (Exception) { }
        }

        private static string Name(ClientCard c) => c == null ? "-" : (c.Name ?? c.Id.ToString());

        private Func<bool> Safe(ExecutorType type, Func<bool> func)
        {
            if (func == null) return null;
            return () =>
            {
                try
                {
                    bool result = func();
                    if (result)
                    {
                        Log($"{type} {Name(Card)} @{Card?.Location}");
                        if (type == ExecutorType.Activate && Card != null)
                        {
                            MerkeEffekt(Card);
                            _aktivId = Card.Id;
                        }
                        if (type == ExecutorType.Summon || type == ExecutorType.MonsterSet) _nsUsed = true;
                        _imIdle = false;
                    }
                    return result;
                }
                catch (Exception e)
                {
                    Log($"FEHLER {type} {Name(Card)}: {e.Message} {e.StackTrace?.Split('\n').FirstOrDefault()}");
                    return false;
                }
            };
        }

        private new void AddExecutor(ExecutorType type, int cardId, Func<bool> func) => base.AddExecutor(type, cardId, Safe(type, func));
        private new void AddExecutor(ExecutorType type, Func<bool> func) => base.AddExecutor(type, Safe(type, func));

        // =====================================================================================
        // Hilfsfunktionen
        // =====================================================================================

        private static bool Is(ClientCard c, int id)
        {
            if (c == null || c.Id == 0) return false;
            if (c.Id == id) return true;
            int alias = c.Data != null ? c.Data.Alias : 0;
            return alias != 0 && alias == id;
        }

        private static K Art(ClientCard c)
        {
            if (c == null) return K.Anderes;
            if (Arten.TryGetValue(c.Id, out K k)) return k;
            if (c.Data != null && c.Data.Alias != 0 && Arten.TryGetValue(c.Data.Alias, out k)) return k;
            return K.Anderes;
        }

        private static O OrtVon(ClientCard c)
        {
            switch (c.Location)
            {
                case CardLocation.Hand: return O.Hand;
                case CardLocation.Deck: return O.Deck;
                case CardLocation.Grave: return O.Grab;
                case CardLocation.MonsterZone: return O.Monster;
                case CardLocation.SpellZone: return O.Zone;
                default: return O.Extra;
            }
        }

        private bool MyMain => Duel.Player == 0 && (Duel.Phase == DuelPhase.Main1 || Duel.Phase == DuelPhase.Main2);
        private bool GegnerMain => Duel.Player == 1 && (Duel.Phase == DuelPhase.Main1 || Duel.Phase == DuelPhase.Main2);
        private static bool Unbeeinflussbar(ClientCard c) => c != null && c.Id == Liger;

        /// <summary>Welcher Einmal-Effekt läuft: Kartenart + Ort der Aktivierung.</summary>
        private void MerkeEffekt(ClientCard c)
        {
            O o = OrtVon(c);
            bool grab = o == O.Grab;
            switch (Art(c))
            {
                case K.Shma: _used.Add(grab ? Planer.E.ShmaZ : Planer.E.ShmaNS); break;
                case K.Shara: _used.Add(grab ? Planer.E.SharaGrab : Planer.E.SharaHand); break;
                case K.Kamura: _used.Add(grab ? Planer.E.KamuraZ : Planer.E.KamuraHand); break;
                case K.Aruha: _used.Add(grab ? Planer.E.AruhaZ : Planer.E.AruhaHand); break;
                case K.Sarama: _used.Add(grab ? Planer.E.SaramaZ : Planer.E.SaramaFeld); break;
                case K.Rakea: _used.Add(grab ? Planer.E.RakeaZ : Planer.E.RakeaFeld); break;
                case K.Sharvara: _used.Add(grab ? Planer.E.SharvaraGrab : Planer.E.SharvaraHand); break;
                case K.Shyama: _used.Add(grab ? Planer.E.ShyamaGrab : Planer.E.ShyamaFeld); break;
                case K.Disaster: _used.Add(grab ? Planer.E.DisasterZ : Planer.E.DisasterLink); break;
                case K.Abominable: _used.Add(o == O.Hand ? Planer.E.AbomSS : Planer.E.AbomPop); break;
                case K.Malice: if (grab) _used.Add(Planer.E.MaliceAdd); break;
                case K.Banish: if (grab) _used.Add(Planer.E.BanishAdd); break;
                case K.Prison: _used.Add(grab ? Planer.E.PrisonZ : Planer.E.PrisonAkt); break;
                case K.Reinc: _used.Add(c.Location == CardLocation.SpellZone && c.IsFaceup() ? Planer.E.ReincFeld : Planer.E.ReincAkt); break;
                case K.Wailing: if (grab) _used.Add(Planer.E.WailingZ); else if (o == O.Zone && c.IsFaceup()) _used.Add(Planer.E.WailingLink); break;
                case K.Chamber: _used.Add(grab ? Planer.E.ChamberZ : Planer.E.ChamberAkt); break;
                case K.Escape: _used.Add(grab ? Planer.E.EscapeZ : Planer.E.EscapeAkt); break;
                case K.Syncretism: _used.Add(grab ? Planer.E.SyncZ : Planer.E.SyncAkt); break;
                case K.SoulLord: _used.Add(grab ? Planer.E.SoulGrab : Planer.E.SoulSuche); break;
                case K.Rage: if (grab) _used.Add(Planer.E.RageZ); break;
                case K.Anguish: _used.Add(grab ? Planer.E.AnguishZ : Planer.E.AnguishLink); break;
                case K.Enma: _used.Add(ActivateDescription == 96 ? Planer.E.EnmaErsatz : Planer.E.EnmaEnde); break;
                case K.Abomination: _used.Add(Planer.E.AbominationZ); break;
            }
        }

        private bool Frei(Planer.E e) => !_used.Contains(e);

        public override bool OnSelectHand()
        {
            Log("=== Neues Duell: UNCHAINED AI ===");
            return true;   // als Erster baut das Deck Enma mit Rage Abominator im Friedhof
        }

        public override void OnNewTurn()
        {
            ZerstoerSchutz.NeuerZug();
            _nsUsed = false;
            _used.Clear();
            _plan.Clear(); _planZ.Clear(); _pos = 0;
            _gruppeAktiv.Clear();
            _queue.Clear();
            _ja.Clear();
            _sharaBeschwoeren = false;
            _neuPlanen = true;
            _main2Geplant = false;
            _schnellStand = null;
            _restGesetzt = false;
            _planZahl = 0;
            _verboten.Clear();
            _zerstoert.Clear();
            _matErwartet.Clear();
            _neuGesetzt.Clear();
            _imIdle = false;
            _impZiel = null;
            _ziel = null;
            base.OnNewTurn();
        }

        public override void OnChaining(int player, ClientCard card)
        {
            base.OnChaining(player, card);
            if (player == 1) Log($"Gegner aktiviert {Name(card)} @{card?.Location}");
        }

        public override void OnChainEnd()
        {
            ZerstoerSchutz.KettenEnde();
            _aktivId = 0;
            base.OnChainEnd();
        }

        /// <summary>Vergleicht das eigene Feld mit der letzten Abfrage: was vom Feld in den Friedhof ging, gilt als zerstört, außer es war
        /// Material der laufenden Link-Beschwörung (Enma belebt nur Zerstörtes). Merkt neu gesetzte Karten. WindBots OnMove fehlt in dieser Bibliothek.</summary>
        private void FeldVergleich()
        {
            var jetzt = new HashSet<ClientCard>(Bot.GetMonsters().Concat(Bot.GetSpells()).Where(c => c != null));
            foreach (var c in _feldLetzt)
            {
                if (jetzt.Contains(c)) continue;
                if (c.Location == CardLocation.Grave && c.Controller == 0)
                {
                    K k = Art(c);
                    if (!_matErwartet.Remove(k)) _zerstoert.Add(c);
                }
                _boden.Remove(c);
            }
            foreach (var c in jetzt)
                if (!_feldLetzt.Contains(c) && c.Location == CardLocation.SpellZone) _neuGesetzt.Add(c);
            _feldLetzt = jetzt;
        }
        private HashSet<ClientCard> _feldLetzt = new HashSet<ClientCard>();

        // =====================================================================================
        // Echter Zustand → Planer
        // =====================================================================================

        private static int GegnerWert(ClientCard c)
        {
            if (c == null) return 0;
            if (!c.HasType(CardType.Monster) && c.Location != CardLocation.MonsterZone) return c.IsFacedown() ? 16 : 12;
            if (c.IsFacedown()) return 16;
#if !OHNE_KLEINWERT
            // Kleine Main-Deck-Monster sind Combo-Teile, oft mit eigenem Auslöser (Rokket): keine Störung und keine Bombe wert
            if (!c.IsExtraCard() && c.Level < 5 && c.Attack < 2000 && !GegnerStoerer.Contains(c.Id)) return 8;
#endif
            int w = 18;
            if (c.IsExtraCard()) w += c.Attack >= 1800 ? 16 : 4;
            if (c.Level >= 8) w += 6;
            if (c.Attack >= 2500) w += 8;
            if (GegnerStoerer.Contains(c.Id)) w += 30;
            if (Unbeeinflussbar(c)) w = 5;
            return w;
        }

        private Planer NeuerPlaner(out Planer.Z start, bool gegnerzug = false)
        {
            _gegner = new List<GegnerKarte>();
            _gegnerRef = new List<ClientCard>();
            foreach (var m in Enemy.GetMonsters())
            {
                if (m == null) continue;
                _gegner.Add(new GegnerKarte
                {
                    Id = m.Id, Wert = GegnerWert(m), Atk = m.Attack, Link = m.HasType(CardType.Link) ? m.LinkCount : 0, Offen = m.IsFaceup(),
                    Immun = Unbeeinflussbar(m) || ZerstoerSchutz.KeinMonsterZiel(m), Unzerst = ZerstoerSchutz.Meiden(m), Monster = true, Verteidigung = m.IsDefense()
                });
                _gegnerRef.Add(m);
            }
            foreach (var s in Enemy.GetSpells())
            {
                if (s == null) continue;
                _gegner.Add(new GegnerKarte { Id = s.Id, Wert = GegnerWert(s), Offen = s.IsFaceup(), Unzerst = ZerstoerSchutz.Meiden(s), Monster = false });
                _gegnerRef.Add(s);
            }
            var pl = gegnerzug
                ? new Planer(_gegner, true, Enemy.LifePoints, null, true, true, GegnerMain)
                : new Planer(_gegner, Duel.Turn == 1, Enemy.LifePoints, _verboten, Duel.Phase == DuelPhase.Main2);
            var z = Planer.Leer();
            var gesehen = new Dictionary<K, int>();
            void Zaehle(K k) { gesehen.TryGetValue(k, out int n); gesehen[k] = n + 1; }
            var hand = new Dictionary<K, int>();
            foreach (var c in Bot.Hand)
            {
                K k = Art(c);
                hand.TryGetValue(k, out int n); hand[k] = n + 1;
                Zaehle(k);
            }
            foreach (var kv in hand) Planer.SetHand(z, kv.Key, kv.Value);
            foreach (var c in Bot.Graveyard)
            {
                K k = Art(c);
                Planer.AddGrab(z, k, _zerstoert.Contains(c));
                Zaehle(k);
            }
            foreach (var c in Bot.Banished) Zaehle(Art(c));
            foreach (var m in Bot.GetMonsters())
            {
                K k = Art(m);
                Planer.AddMonster(z, k, m.IsDisabled(), _boden.Contains(m), m.IsFacedown());
                Zaehle(k);
            }
            foreach (var s in Bot.GetSpells())
            {
                K k = Art(s);
                Planer.AddZone(z, k, s.IsFacedown(), _neuGesetzt.Contains(s));
                Zaehle(k);
            }
            foreach (var kv in Copies)
            {
                gesehen.TryGetValue(kv.Key, out int n);
                Planer.SetDeck(z, kv.Key, kv.Value - n);
            }
            foreach (K link in new[] { K.SoulLord, K.Rage, K.Anguish, K.Abominator, K.Enma, K.Abomination })
                Planer.SetDeck(z, link, Bot.ExtraDeck.Count(c => Art(c) == link));
            if (_nsUsed) Planer.NormalVerbraucht(z);
            foreach (var e in _used) Planer.Benutzt(z, e);
            pl.SetGegner(z);
            start = z;
            return pl;
        }

        private void Planen(string grund)
        {
            FeldVergleich();
            _planZahl++;
            var uhr = System.Diagnostics.Stopwatch.StartNew();
            var pl = NeuerPlaner(out var start);
            _planZ = pl.Suche(start, _planZahl <= 1 ? 6000 : 3000, _planZahl <= 1 ? 2500 : 1500, out int wert);
            _plan = _planZ.Select(z => z.Schritt).ToList();
            _pos = 0;
            _gruppeAktiv.Clear();
            _queue.Clear();
            _ja.Clear();
            _neuPlanen = false;
            Log($"PLAN ({grund}) Wert {wert}, {pl.Knoten} Knoten, {uhr.ElapsedMilliseconds} ms, Start {Planer.Signatur(start)}");
            for (int i = 0; i < _plan.Count; i++) Log($"  {i + 1}. {_plan[i]}");
        }

        // =====================================================================================
        // Hauptphase: Plan prüfen und abarbeiten
        // =====================================================================================

        private bool IstAusloeser(Schritt s) => s.Art == Decks.Art.Ausloeser || s.Art == Decks.Art.Auslassen;

        /// <summary>Läuft bei jeder Abfrage in der Hauptphase als Erstes, beantwortet nie (false).</summary>
        private bool IdleHook()
        {
            try
            {
                _imIdle = true;
                _angeboten = null;
                FeldVergleich();
                _queue.Clear();
                _ja.Clear();
                _matErwartet.Clear();
                _sharaBeschwoeren = false;
                _gruppeAktiv.Clear();
                if (!MyMain) return false;
                if (ErstBattlePhase()) return false;
                while (_pos < _plan.Count && IstAusloeser(_plan[_pos])) _pos++;     // abgearbeitete Ketten überspringen
                bool neu = _neuPlanen;
                if (!neu && _plan.Count > 0 && !_restGesetzt)
                {
                    // Der Stand muss zum Plan passen, sonst neu planen (Handfalle, Sonderfall)
                    NeuerPlaner(out var ist);
                    var soll = _pos > 0 ? _planZ[_pos - 1] : null;
                    if (soll != null && Planer.Signatur(soll) != Planer.Signatur(ist))
                    {
                        Log($"Plan passt nicht mehr: soll {Planer.Signatur(soll)} | ist {Planer.Signatur(ist)}");
                        neu = true;
                    }
                }
                if (!neu && _plan.Count == 0 && _planZahl == 0) neu = true;
                if (!neu && _pos >= _plan.Count && _planZahl < 12)
                {
                    // Plan fertig: nach der Battle Phase noch einmal schauen, ob etwas geht
                    if (Duel.Phase == DuelPhase.Main2 && !_main2Geplant) { _main2Geplant = true; neu = true; }
                }
                if (neu && _planZahl < 12) Planen(_neuPlanen ? "neu" : "Abweichung");
                // Nächster Schritt nicht verfügbar → verbieten und neu planen
                for (int v = 0; v < 6 && _pos < _plan.Count; v++)
                {
                    var s = _plan[_pos];
                    if (IstAusloeser(s) || Verfuegbar(s)) break;
                    Log($"Schritt nicht verfügbar: {s}");
                    _verboten.Add(s.Art + " " + s.Karte + " " + s.Ort);
                    if (_planZahl >= 12) { _plan.Clear(); _planZ.Clear(); _pos = 0; break; }
                    Planen("nicht verfügbar");
                }
            }
            catch (Exception e) { Log($"FEHLER Hook: {e.Message} {e.StackTrace?.Split('\n').FirstOrDefault()}"); }
            return false;
        }
        private bool _main2Geplant, _restGesetzt;

        private bool Verfuegbar(Schritt s)
        {
            var main = Duel.MainPhase;
            if (main == null) return true;
            switch (s.Art)
            {
                case Decks.Art.Normal: return main.SummonableCards.Any(c => Art(c) == s.Karte);
                case Decks.Art.Setzen: return main.SpellSetableCards.Any(c => Art(c) == s.Karte);
                case Decks.Art.Link: return main.SpecialSummonableCards.Any(c => Art(c) == s.Karte);
                case Decks.Art.Aktivieren: return main.ActivableCards.Any(c => Art(c) == s.Karte && OrtVon(c) == s.Ort);
            }
            return true;
        }

        private void Queue(Schritt s, bool vorne)
        {
            if (vorne) _queue.InsertRange(0, s.Wahlen);
            else _queue.AddRange(s.Wahlen);
            if (s.Ja) _ja.Add(s.Karte);
            if (s.Beschwoeren) _sharaBeschwoeren = true;
            if (s.Ziehen) _neuPlanen = true;
        }

        private bool PlanNormal()
        {
            if (!_imIdle || _pos >= _plan.Count) return false;
            var s = _plan[_pos];
            if (s.Art != Decks.Art.Normal || Art(Card) != s.Karte) return false;
            _pos++;
            return true;
        }

        private bool PlanSetzen()
        {
            if (!_imIdle || _pos >= _plan.Count) return false;
            var s = _plan[_pos];
            if (s.Art != Decks.Art.Setzen || Art(Card) != s.Karte) return false;
            _pos++;
            return true;
        }

        private bool PlanLink()
        {
            if (!_imIdle || _pos >= _plan.Count) return false;
            var s = _plan[_pos];
            if (s.Art != Decks.Art.Link || Art(Card) != s.Karte) return false;
            _pos++;
            Queue(s, false);
            _matErwartet.AddRange(s.Wahlen.Select(w => w.Karte));
            return true;
        }

        private bool PlanAktivieren()
        {
            if (Card == null) return false;
            if (_imIdle)
            {
                if (_pos >= _plan.Count) return false;
                var s = _plan[_pos];
                if (s.Art != Decks.Art.Aktivieren || Art(Card) != s.Karte || OrtVon(Card) != s.Ort) return false;
                _pos++;
                Queue(s, false);
                return true;
            }
            // Schnellplan im Gegnerzug: erster Schritt ist ein schneller Effekt
            if (Duel.Player == 1 && _pos < _plan.Count && _plan[_pos].Art == Decks.Art.Aktivieren)
            {
                var s = _plan[_pos];
                if (Art(Card) != s.Karte || OrtVon(Card) != s.Ort) return false;
                _pos++;
                Queue(s, false);
                Log($"Schnellplan: {s}");
                return true;
            }
            // Auslöser: die laufende Gruppe des Plans
            if (!MyMain && Duel.Player != 1) return false;
            var gruppe = LaufendeGruppe();
            if (gruppe.Count == 0) return false;
            K k = Art(Card); O o = OrtVon(Card);
            int hier = gruppe.FindIndex(i => _plan[i].Karte == k && _plan[i].Ort == o);
            if (hier < 0 || _plan[gruppe[hier]].Art == Decks.Art.Auslassen) return false;
            // Aktiviert wird rückwärts: die letzte Auflösung geht als erstes Kettenglied in die Kette.
            for (int j = gruppe.Count - 1; j >= 0; j--)
            {
                int i = gruppe[j];
                var s = _plan[i];
                if (s.Art == Decks.Art.Auslassen || _gruppeAktiv.Contains(i)) continue;
                bool angeboten = _angeboten == null || !_angeboten.Contains(Card)
                    ? s.Karte == k && s.Ort == o
                    : _angeboten.Any(c => Art(c) == s.Karte && OrtVon(c) == s.Ort);
                if (!angeboten) continue;
                if (s.Karte != k || s.Ort != o) return false;      // erst das andere Kettenglied
                _gruppeAktiv.Add(i);
                Queue(s, true);
                return true;
            }
            return false;
        }

        /// <summary>Indizes der Auslöser-Schritte der nächsten Kette; fertige Ketten werden übersprungen.</summary>
        private List<int> LaufendeGruppe()
        {
            while (true)
            {
                var liste = new List<int>();
                if (_pos >= _plan.Count || !IstAusloeser(_plan[_pos])) return liste;
                int g = _plan[_pos].Gruppe;
                for (int i = _pos; i < _plan.Count && IstAusloeser(_plan[i]) && _plan[i].Gruppe == g; i++) liste.Add(i);
                if (liste.All(i => _plan[i].Art == Decks.Art.Auslassen || _gruppeAktiv.Contains(i)) && liste.Any(i => _gruppeAktiv.Contains(i)))
                {
                    _pos = liste.Last() + 1;
                    _gruppeAktiv.Clear();
                    continue;
                }
                return liste;
            }
        }

        /// <summary>Plan lehnt diesen Auslöser ausdrücklich ab (z. B. Rage Abominator bei eigener Enma, Soul Lord aufsparen).</summary>
        private bool PlanLehntAb()
        {
            if (!MyMain && Duel.Player != 1) return false;
            var gruppe = LaufendeGruppe();
            K k = Art(Card); O o = OrtVon(Card);
            return gruppe.Any(i => _plan[i].Art == Decks.Art.Auslassen && _plan[i].Karte == k && _plan[i].Ort == o);
        }

        public override void OnSelectChain(IList<ClientCard> cards)
        {
            _imIdle = false;
            _angeboten = cards;
            FeldVergleich();
#if !OHNE_SCHNELLPLAN
            try { Schnellplan(cards); }
            catch (Exception e) { Log($"FEHLER Schnellplan: {e.Message} {e.StackTrace?.Split('\n').FirstOrDefault()}"); }
#endif
            base.OnSelectChain(cards);
        }

        private string _schnellStand;

        /// <summary>Gegnerzug: Lohnt sich jetzt ein schneller Effekt? Der Planer sucht mit Rakea, Sharvara, Shara und gesetzten Fallen (z. B. Rakea holt Kamura,
        /// zweite Rakea sprengt sie). Nur gegen lohnende Ziele, sonst verpuffen die Karten an Combo-Teilen (Ketos Rokket-Duelle 02.11.).</summary>
        private void Schnellplan(IList<ClientCard> cards)
        {
            if (Duel.Player != 1) return;
            if (LaufendeGruppe().Any(i => cards.Any(c => Art(c) == _plan[i].Karte && OrtVon(c) == _plan[i].Ort))) return;
            bool kandidat = cards.Any(c => Is(c, Rakea) && c.Location == CardLocation.MonsterZone
                || (Is(c, Sharvara) || Is(c, Shara)) && c.Location == CardLocation.Hand
                || (Is(c, Escape) || Is(c, Chamber)) && c.Location == CardLocation.SpellZone);
            if (!kandidat) return;
            if (!Enemy.GetMonsters().Any(m => m != null && GegnerWert(m) >= 34)) return;
            string stand = string.Join(",", Enemy.GetMonsters().Where(m => m != null).Select(m => m.Id)) + "|" + Bot.GetMonsterCount() + "|" + Bot.GetSpellCount() + "|" + Bot.Hand.Count + "|" + Duel.Phase;
            if (stand == _schnellStand) return;
            _schnellStand = stand;
            var pl = NeuerPlaner(out var start, true);
            int basis = pl.Bewerte(start);
            var pfad = pl.Suche(start, 1500, 500, out int wert);
            if (pfad.Count == 0 || wert - basis < 15 || pfad[0].Schritt.Art != Decks.Art.Aktivieren) return;
            _planZ = pfad;
            _plan = pfad.Select(z => z.Schritt).ToList();
            _pos = 0;
            _gruppeAktiv.Clear();
            _queue.Clear();
            _ja.Clear();
            Log($"SCHNELLPLAN Wert {basis} → {wert}, {pl.Knoten} Knoten");
            for (int i = 0; i < _plan.Count; i++) Log($"  {i + 1}. {_plan[i]}");
        }

        // =====================================================================================
        // Regeln außerhalb des Plans
        // =====================================================================================

        private bool ErstBattlePhase()
        {
            // Evenly Matched von der Hand: Feld leer lassen, Battle Phase, an deren Ende aktivieren, danach die Combo in Main Phase 2 (Ketos Linie 19:41)
            if (Duel.Turn == 1 || Duel.Phase != DuelPhase.Main1 || Duel.Player != 0) return false;
            if (!Bot.Hand.Any(c => Is(c, Evenly)) || Bot.GetFieldCount() > 0) return false;
            return Enemy.GetFieldCount() >= 3;
        }

        private bool EvenlyEffekt()
        {
            return Duel.Player == 0 && Enemy.GetFieldCount() >= Bot.GetFieldCount() + 2;
        }

        private bool EnmaEffekt()
        {
            // End Phase: 2 zerstörte Fiends beleben. Rage Abominator bleibt im Friedhof, dort stört er.
            if (ActivateDescription != 96) return Card.Location == CardLocation.MonsterZone && _zerstoert.Any(c => c.Location == CardLocation.Grave && Art(c) != K.Abominator);
            // „Stattdessen 1 offene Karte zerstören“: zuerst eine Gegnerkarte (Simulator 02.11.: Baronne starb statt Enma)
            _ziel = ErsatzZiel();
            if (_ziel == null) return false;
            Log($"Enmas Schutz zerstört {Name(_ziel)}");
            return true;
        }

        private ClientCard ErsatzZiel()
        {
            var gegner = Enemy.GetMonsters().Concat(Enemy.GetSpells()).Where(c => c != null && c.IsFaceup() && !Unbeeinflussbar(c) && !ZerstoerSchutz.Meiden(c, ZerstoerSchutz.AlsMonster))
                .OrderByDescending(GegnerWert).FirstOrDefault();
            if (gegner != null) return gegner;
            return Bot.GetSpells().Where(c => c != null && c.IsFaceup()).FirstOrDefault()
                ?? Bot.GetMonsters().Where(c => c != null && c.IsFaceup() && !Is(c, Enma)).OrderBy(EigenWert).FirstOrDefault();
        }

        /// <summary>Banish aus dem Friedhof: zerstört das Monster, das den Effekt aktiviert. Nie bei eigener Aktivierung.</summary>
        private bool AbominatorEffekt()
        {
            if (Card.Location != CardLocation.Grave) return false;
            var last = Util.GetLastChainCard();
            if (last == null || last.Controller != 1 || last.Location != CardLocation.MonsterZone || Unbeeinflussbar(last) || ZerstoerSchutz.Verpufft(last, ZerstoerSchutz.AlsMonster)) return false;
            // Sparsam: Der Effekt negiert nicht und geht nur 1×. Ketos Rokket-Duelle 02.11.: verschwendet auf Striker Dragon und Rokket Loader,
            // Trisborrel und Borrelend wirkten danach ungestört.
#if !OHNE_ABOMWERT
            if (GegnerWert(last) >= 34 || last.Level >= 8) return true;
            return (Duel.ChainTargets ?? new List<ClientCard>()).Any(c => c != null && c.Controller == 0 && (Is(c, Enma) || Is(c, Abomination) || Is(c, AbominationAlt)));
#else
            if (last.IsExtraCard() || last.Level >= 5 || last.Attack >= 2000 || GegnerStoerer.Contains(last.Id)) return true;
            return Enemy.GetMonsterCount() >= 3;
#endif
        }

        private bool ImpermEffekt()
        {
            if (Card.Location == CardLocation.Hand) return false;
            if (Card.IsDisabled()) return false;
            var last = Util.GetLastChainCard();
            if (Duel.LastChainPlayer == 1 && last != null && last.Location == CardLocation.MonsterZone && last.IsFaceup() && !last.IsDisabled()
                && !Unbeeinflussbar(last) && last.Controller == 1
#if !OHNE_KLEINWERT
                && GegnerWert(last) >= 18
#endif
                )
            {
                _impZiel = last;
                AI.SelectCard(last);
                return true;
            }
            // Gegnerzug: das gefährlichste offene Monster, bevor es wirkt
            if (Duel.Player == 1 && (Duel.Phase == DuelPhase.Main1 || Duel.Phase == DuelPhase.BattleStart))
            {
                var z = Enemy.GetMonsters().Where(m => m != null && m.IsFaceup() && !m.IsDisabled() && !Unbeeinflussbar(m) && GegnerWert(m) >= 34)
                    .OrderByDescending(GegnerWert).FirstOrDefault();
                if (z != null) { _impZiel = z; AI.SelectCard(z); return true; }
            }
            return false;
        }

        private bool MaliceEffekt()
        {
            if (Card.Location == CardLocation.Grave) return true;  // Zurück auf die Hand
            if (Duel.LastChainPlayer != 1) return false;
            return Bot.Graveyard.Any(c => IstStufe8(c)) && Bot.GetMonsterCount() < 5;
        }
        private static bool IstStufe8(ClientCard c) => Is(c, Kamura) || Is(c, Disaster) || Is(c, Abominable);

        private bool RageEffekt()
        {
            if (Card.Location == CardLocation.Grave) return true;   // Fiend auf die Hand
            if (!GegnerMain) return false;
            var z = Enemy.GetMonsters().Where(m => m != null && m.IsFaceup() && !Unbeeinflussbar(m) && (m.IsExtraCard() || m.Attack >= 2000 || GegnerStoerer.Contains(m.Id)))
                .OrderByDescending(GegnerWert).FirstOrDefault();
            if (z == null) return false;
            _ziel = z;
            Log($"Rage nimmt {Name(z)} als Material");
            return true;
        }

        private bool EscapeEffekt()
        {
            if (Card.Location == CardLocation.Grave) return !PlanLehntAb();
            if (Duel.Player == 0 && _imIdle) return false;  // im eigenen Zug plant der Planer
            var ziele = Enemy.GetMonsters().Concat(Enemy.GetSpells()).Where(c => c != null && !Unbeeinflussbar(c) && !ZerstoerSchutz.Meiden(c, ZerstoerSchutz.AlsFalle)).OrderByDescending(GegnerWert).ToList();
            var eigen = EigenesFutter(true);
            if (ziele.Count == 0 || eigen == null) return false;
            if (Duel.Player != 1 || GegnerWert(ziele[0]) < 34) return false;
            _ziel = ziele[0];
            AI.SelectCard(eigen);
            AI.SelectNextCard(ziele[0]);
            return true;
        }

        private bool SyncretismEffekt()
        {
            if (Card.Location == CardLocation.Grave) return !PlanLehntAb();
            if (!GegnerMain) return false;
            var seele = Bot.GetMonsters().FirstOrDefault(m => m != null && m.IsFaceup() && (Is(m, SoulLord) || Is(m, Rage) || Is(m, Anguish) || Is(m, Abominator)));
            if (seele == null) return false;
            var z = Enemy.GetMonsters().Where(m => m != null && m.IsFaceup() && !Unbeeinflussbar(m)).OrderByDescending(GegnerWert).FirstOrDefault();
            if (z == null || GegnerWert(z) < 30) return false;
            AI.SelectCard(seele);
            AI.SelectNextCard(z);
            return true;
        }

        private bool ChamberEffekt()
        {
            if (Card.Location == CardLocation.Grave) return !PlanLehntAb();
            if (Duel.Player != 1) return false;
            // Im Gegnerzug: Blocker, wenn die Battle Phase kommt und das Feld leer ist, sonst in der End Phase Körper holen
            if (Duel.Phase == DuelPhase.BattleStart && Bot.GetMonsterCount() == 0) return true;
            return Duel.Phase == DuelPhase.End && Bot.Graveyard.Any(c => Arten.ContainsKey(c.Id) && Art(c) <= K.Abominable);
        }

        /// <summary>Eigene Karte als Zerstörungsziel, die etwas auslöst: niedrig = besser.</summary>
        private int EigenWert(ClientCard c)
        {
            K k = Art(c);
            if (k == K.Enma || k == K.Abomination) return 1000;
            if (c.Location == CardLocation.SpellZone)
            {
                if (c.IsFacedown())
                {
                    switch (k)
                    {
                        case K.Prison: return Frei(Planer.E.PrisonZ) ? 2 : 40;
                        case K.Wailing: return Frei(Planer.E.WailingZ) ? 3 : 40;
                        case K.Chamber: return Frei(Planer.E.ChamberZ) ? 4 : 40;
                        case K.Escape: return Frei(Planer.E.EscapeZ) ? 4 : 40;
                        case K.Syncretism: return Frei(Planer.E.SyncZ) ? 4 : 40;
                        default: return 60;
                    }
                }
                return 80;
            }
            switch (k)
            {
                case K.Shma: return Frei(Planer.E.ShmaZ) ? 5 : 45;
                case K.Aruha: return Frei(Planer.E.AruhaZ) ? 6 : 45;
                case K.Sarama: return Frei(Planer.E.SaramaZ) ? 6 : 45;
                case K.Rakea: return Frei(Planer.E.RakeaZ) && Card != null && !Is(Card, Rakea) ? 7 : 50;
                case K.Disaster: return Frei(Planer.E.DisasterZ) ? 10 : 45;
                case K.Rage: return Frei(Planer.E.RageZ) ? 12 : 45;
                case K.Anguish: return Frei(Planer.E.AnguishZ) ? 12 : 45;
                case K.Kamura: return 90;
                default: return 50;
            }
        }

        private ClientCard EigenesFutter(bool nurMonster)
        {
            IEnumerable<ClientCard> pool = Bot.GetMonsters().Where(m => m != null && (m.IsFaceup() || !nurMonster));
            if (!nurMonster) pool = pool.Concat(Bot.GetSpells().Where(s => s != null));
            return pool.Where(c => EigenWert(c) < 100).OrderBy(EigenWert).FirstOrDefault();
        }

        /// <summary>Gegner zielt auf eine eigene Karte oder Kamura kann sprengen: selbst zerstören und den Auslöser nutzen.</summary>
        private ClientCard ReaktionsZiel()
        {
            if (Duel.LastChainPlayer == 1)
            {
                var ziel = (Duel.ChainTargets ?? new List<ClientCard>()).FirstOrDefault(c => c != null && c.Controller == 0
                    && (c.Location == CardLocation.MonsterZone || c.Location == CardLocation.SpellZone) && !Is(c, Enma));
                if (ziel != null && !Is(ziel, Rakea)) return ziel;
            }
            return null;
        }

        /// <summary>Lohnt die Kamura-Bombe im Gegnerzug? Gegner verliert mehr als ich (Enma überlebt durch den Ersatz).</summary>
        private bool BombeLohnt()
        {
            var gegner = Enemy.GetMonsters().Where(m => m != null && !Unbeeinflussbar(m) && !ZerstoerSchutz.Verpufft(m, ZerstoerSchutz.AlsMonster)).ToList();
            if (gegner.Count == 0) return false;
            int seine = gegner.Sum(GegnerWert);
            int meine = Bot.GetMonsters().Where(m => m != null && !(Is(m, Enma) && Frei(Planer.E.EnmaErsatz))).Sum(m => 10 + (m.HasType(CardType.Link) ? 15 : 0));
            return seine > meine;
        }

#if !OHNE_DODGE
        /// <summary>Eigenes offenes Monster, auf das der Gegner gerade zielt und das als Antwort selbst zerstört werden kann. Nicht Enma und nicht die eigene Karte.</summary>
        private ClientCard AusweichZiel()
        {
            if (Duel.LastChainPlayer != 1) return null;
            return (Duel.ChainTargets ?? new List<ClientCard>()).FirstOrDefault(c => c != null && c != Card && c.Controller == 0 && c.Location == CardLocation.MonsterZone
                && c.IsFaceup() && !Is(c, Enma) && !Is(c, Abomination) && !Is(c, AbominationAlt));
        }
#endif

        private bool RakeaEffekt()
        {
            if (Card.Location == CardLocation.Grave) return !PlanLehntAb();
#if !OHNE_DODGE
            // Im eigenen Zug ausweichen: Der Gegner zielt mit Impermanence oder Veiler auf ein Monster → selbst zerstören, die Falle verpufft (Tims Guide 03.11.)
            if (Duel.Player == 0 && !_imIdle && !Card.IsDisabled()) { var d = AusweichZiel(); if (d != null) { _ziel = d; AI.SelectCard(d); Log($"Rakea weicht aus: {Name(d)}"); return true; } }
#endif
            if (Duel.Player != 1 || Card.IsDisabled()) return false;
            var r = ReaktionsZiel();
            if (r != null) { _ziel = r; AI.SelectCard(r); return true; }
#if OHNE_SCHNELLPLAN
            var kamura = Bot.GetMonsters().FirstOrDefault(m => m != null && Is(m, Kamura));
            if (kamura != null && Frei(Planer.E.KamuraZ) && BombeLohnt() && GegnerMain) { AI.SelectCard(kamura); Log("Rakea sprengt Kamura"); return true; }
#endif
            // End Phase des Gegners: gesetzte Karte zerstören → Monster aus dem Deck (kostet nichts, alles ist im nächsten Zug wieder frei)
            if (Duel.Phase == DuelPhase.End)
            {
                var f = Bot.GetSpells().Where(s => s != null && s.IsFacedown() && EigenWert(s) < 10 && !Is(s, Chamber) && !Is(s, Escape)).OrderBy(EigenWert).FirstOrDefault();
                if (f != null && Bot.GetMonsterCount() < 5) { AI.SelectCard(f); return true; }
            }
            return false;
        }

        private bool SharvaraEffekt()
        {
            if (Card.Location == CardLocation.Grave) return !PlanLehntAb();
#if !OHNE_DODGE
            if (Duel.Player == 0 && !_imIdle) { var d = AusweichZiel(); if (d != null) { _ziel = d; AI.SelectCard(d); Log($"Sharvara weicht aus: {Name(d)}"); return true; } }
#endif
            if (Duel.Player != 1) return false;
            var r = ReaktionsZiel();
            if (r != null && (r.IsFacedown() || r.Location == CardLocation.MonsterZone)) { AI.SelectCard(r); return true; }
#if OHNE_SCHNELLPLAN
            var kamura = Bot.GetMonsters().FirstOrDefault(m => m != null && Is(m, Kamura) && m.IsFaceup());
            if (kamura != null && Frei(Planer.E.KamuraZ) && BombeLohnt() && GegnerMain) { AI.SelectCard(kamura); Log("Sharvara sprengt Kamura"); return true; }
#endif
            return false;
        }

        private bool SharaEffekt()
        {
            if (Card.Location == CardLocation.Grave) return !PlanLehntAb();
            if (Duel.Player != 1) return false;
            // Shara: Fiend von der Hand beschwören, 1 eigene Karte zerstören. Nur wenn ein Ziel gerade getroffen wird.
            if (!Bot.Hand.Any(c => c != Card && Arten.ContainsKey(c.Id) && Art(c) <= K.Banish)) return false;
            var r = ReaktionsZiel();
            if (r != null) { _ziel = r; return true; }
            return false;
        }

        private bool KamuraBombeEffekt()
        {
            if (Card.Location != CardLocation.Grave) return false;
            if (MyMain && PlanLehntAb()) return false;
            if (MyMain && LaufendeGruppe().Any(i => _plan[i].Karte == K.Kamura)) return false;
            return BombeLohnt() || Duel.Player == 1 && ZerstoerSchutz.HatZiel(Enemy.GetMonsters(), ZerstoerSchutz.AlsMonster) && Bot.GetMonsterCount() <= 1;
        }

        /// <summary>Auslöser, die der Plan nicht vorhergesehen hat (Gegnerzug, Abweichung): nutzen, kosten nichts.</summary>
        private bool RegelAusloeser()
        {
            if (_imIdle || Card == null) return false;
            K k = Art(Card); O o = OrtVon(Card);
            if (PlanLehntAb()) return false;
            switch (k)
            {
                case K.Shma: case K.Aruha: case K.Sarama: case K.Rakea: return o == O.Grab || k == K.Shma && o == O.Monster;
                case K.Prison: case K.Wailing: case K.Chamber: case K.Escape: case K.Syncretism: return o == O.Grab;
                case K.Sharvara: case K.Shara: return o == O.Grab;
                case K.Disaster: return o == O.Grab;
                case K.Rage: case K.Anguish: return o == O.Grab;
                case K.Malice: case K.Banish: return o == O.Grab;
                case K.SoulLord: return o == O.Monster || o == O.Grab;
                case K.Abominable: return o == O.Hand || o == O.Monster && Bot.Hand.Count > 0 && ZerstoerSchutz.HatZiel(Enemy.GetMonsters().Concat(Enemy.GetSpells()), ZerstoerSchutz.AlsMonster) || o == O.Grab;
                case K.Abomination: return ZerstoerSchutz.HatZiel(Enemy.GetMonsters().Concat(Enemy.GetSpells()), ZerstoerSchutz.AlsMonster);
                default: return false;
            }
        }

        private bool RestSetzen()
        {
            if (!MyMain || _pos < _plan.Count) return false;
            if (Is(Card, Evenly) && Duel.Turn > 1 && (Bot.GetFieldCount() == 0 || Duel.Phase == DuelPhase.Main1 && Bot.HasAttackingMonster())) return false;
            bool setzen = Is(Card, Imperm) || Is(Card, Chamber) || Is(Card, Escape) || Is(Card, Syncretism) || Is(Card, Wailing) || Is(Card, Evenly);
            if (setzen) _restGesetzt = true;
            return setzen;
        }

        private bool MonsterRepos()
        {
            if (Duel.Turn == 1) return false;
            return DefaultMonsterRepos();
        }

        // =====================================================================================
        // Fragen beantworten
        // =====================================================================================

        public override CardPosition OnSelectPosition(int cardId, IList<CardPosition> positions)
        {
            NamedCard data = NamedCard.Get(cardId);
            if (data != null && (data.Type & (int)CardType.Link) != 0) return CardPosition.FaceUpAttack;
            if (Duel.Player == 1 || Duel.Turn == 1 || Duel.Phase == DuelPhase.End) return CardPosition.FaceUpDefence;
            if (data != null && data.Attack >= 1800 || Enemy.GetMonsterCount() == 0) return CardPosition.FaceUpAttack;
            return CardPosition.FaceUpDefence;
        }

        public override bool OnSelectYesNo(long desc)
        {
            try
            {
                if (desc == Util.GetStringId(Shyama, 2))
                    return _ja.Contains(K.Shyama) || _plan.Count == 0 && ZerstoerSchutz.HatZiel(Enemy.GetSpells(), ZerstoerSchutz.AlsMonster);
                if (desc == Util.GetStringId(SoulLord, 2)) return _ja.Contains(K.SoulLord);
                if (desc == Util.GetStringId(Reinc, 2)) return true;
                if (desc == Util.GetStringId(Reinc, 3)) return _ja.Contains(K.Reinc);
                if (desc == Util.GetStringId(Kamura, 2)) return Bot.Deck.Count > 3;
                if (desc == Util.GetStringId(Kamura, 3)) return false;
                if (desc == Util.GetStringId(Kamura, 4)) return _ja.Contains(K.Kamura) || _queue.Count == 0 && ZerstoerSchutz.HatZiel(Enemy.GetMonsters().Concat(Enemy.GetSpells()), ZerstoerSchutz.AlsMonster);
            }
            catch (Exception e) { Log($"FEHLER JaNein: {e.Message}"); }
            return base.OnSelectYesNo(desc);
        }

        public override int OnSelectOption(IList<long> options)
        {
            try
            {
                // Shara aus dem Friedhof: auf die Hand (573) oder beschwören
                int sp = options.IndexOf(Util.GetStringId(Shara, 3));
                if (sp >= 0)
                {
                    bool beschwoeren = _sharaBeschwoeren || Duel.Player == 1 && Bot.GetMonsterCount() == 0;
                    _sharaBeschwoeren = false;
                    if (beschwoeren) return sp;
                    int hand = options.IndexOf(573);
                    return hand >= 0 ? hand : 1 - sp;
                }
            }
            catch (Exception e) { Log($"FEHLER Option: {e.Message}"); }
            return base.OnSelectOption(options);
        }

        public override int OnSelectPlace(long cardId, int player, CardLocation location, int available)
        {
            try
            {
                if (player == 0 && location == CardLocation.MonsterZone)
                {
                    NamedCard data = NamedCard.Get((int)cardId);
                    bool link = data != null && (data.Type & (int)CardType.Link) != 0;
                    int emz = available & ((1 << 5) | (1 << 6));
                    if (link && emz != 0) return (emz & (1 << 5)) != 0 ? 1 << 5 : 1 << 6;
                    int main = available & 0x1f;
                    for (int i = 0; i < 5; i++) if ((main & (1 << i)) != 0) return 1 << i;
                }
            }
            catch (Exception e) { Log($"FEHLER Zone: {e.Message}"); }
            return base.OnSelectPlace(cardId, player, location, available);
        }

        // =====================================================================================
        // Karten auswählen
        // =====================================================================================

        private ClientCard Treffer(Wahl w, IList<ClientCard> cards)
        {
            if (w.Gegner >= 0)
            {
                if (w.Gegner < _gegnerRef.Count && cards.Contains(_gegnerRef[w.Gegner])) return _gegnerRef[w.Gegner];
                return null;
            }
            var passend = cards.Where(c => c.Controller == 0 && Art(c) == w.Karte).ToList();
            if (passend.Count == 0) return null;
            var ort = passend.Where(c => OrtVon(c) == w.Ort).ToList();
            if (ort.Count == 0) ort = passend;
            var gleich = ort.Where(c => c.Location == CardLocation.Deck || c.Location == CardLocation.Hand || c.IsFacedown() == (w.Verdeckt != 0)).ToList();
            return (gleich.Count > 0 ? gleich : ort).FirstOrDefault();
        }

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            try
            {
                IList<ClientCard> result = null;
                try { result = Auswahl(cards, min, max, hint); }
                catch (Exception e) { Log($"FEHLER Auswahl {hint}: {e.Message} {e.StackTrace?.Split('\n').FirstOrDefault()}"); }
                result = ZerstoerSchutz.Pruefe(result, cards, min, max, hint, AI.HaveSelectedCards(), Duel.CurrentChain.LastOrDefault(k => k != null && k.Controller == 0));
                if (result != null)
                {
                    Log($"  Auswahl {hint} ({min}-{max}): {string.Join(", ", result.Select(Name))}");
                    return result;
                }
                Log($"  Auswahl {hint} ({min}-{max}) STANDARD aus: {string.Join(", ", cards.Select(c => $"{Name(c)}@{c.Location}{(c.Controller == 1 ? "/G" : "")}"))}");
                return base.OnSelectCard(cards, min, max, hint, cancelable);
            }
            finally
            {
                // Nach dem Mischen stimmen gemerkte IDs der Deck-Plätze nicht mehr (siehe SUN GOD AI).
                foreach (ClientCard c in Bot.Deck)
                    if (c != null && c.Id != 0)
                        c.SetId(0);
            }
        }

        private IList<ClientCard> Auswahl(IList<ClientCard> cards, int min, int max, long hint)
        {
            // 1. Plan
            var gewaehlt = new List<ClientCard>();
            for (int i = 0; i < _queue.Count && gewaehlt.Count < max; )
            {
                var c = Treffer(_queue[i], cards.Where(x => !gewaehlt.Contains(x)).ToList());
                if (c != null) { gewaehlt.Add(c); _queue.RemoveAt(i); }
                else i++;
            }
            if (gewaehlt.Count >= Math.Max(1, min)) return gewaehlt;
            if (gewaehlt.Count > 0) return Util.CheckSelectCount(gewaehlt, cards, min, max);
            // Freiwillige weitere Auswahl (Kamura „bis zu 3“, weiteres Link-Material): nichts dazunehmen
            if (min == 0) return new List<ClientCard>();
            // 2. Vorwahl aus einer Regel
            if (AI.HaveSelectedCards()) return null;
            // 3. Regeln
            return RegelWahl(cards, min, max, hint);
        }

        private static int SsPrioWert(K k)
        {
            switch (k)
            {
                case K.Kamura: return 0;
                case K.Rakea: return 1;
                case K.Abominable: return 2;
                case K.Sharvara: return 3;
                case K.Shyama: return 4;
                case K.Aruha: return 5;
                case K.Sarama: return 6;
                case K.Shma: return 7;
                case K.Disaster: return 8;
                case K.Shara: return 9;
                default: return 20;
            }
        }
        private static int HandWert(K k)
        {
            switch (k)
            {
                case K.Sharvara: return 0;
                case K.Kamura: return 1;
                case K.Shara: return 2;
                case K.Shma: return 3;
                case K.Aruha: return 4;
                case K.Rakea: return 5;
                case K.Abominable: return 6;
                case K.Shyama: return 7;
                case K.Sarama: return 8;
                default: return 20;
            }
        }
        private static int EnmaZielWert(K k)
        {
            switch (k)
            {
                case K.Rakea: return 0;
                case K.Kamura: return 1;
                case K.Abominable: return 2;
                case K.Sarama: return 3;
                case K.Aruha: return 4;
                case K.Sharvara: return 5;
                case K.Shyama: return 6;
                case K.Shma: return 7;
                case K.Disaster: return 8;
                case K.Shara: return 9;
                default: return 15;
            }
        }
        private static int AbwurfWert(K k)
        {
            switch (k)
            {
                case K.Sharvara: return 0;      // setzt aus dem Friedhof
                case K.Banish: return 1;
                case K.Evenly: return 2;
                case K.Wailing: return 3;
                case K.Syncretism: return 4;
                case K.Sarama: return 5;
                case K.Malice: return 6;
                case K.Shara: return 7;
                case K.Aruha: return 8;
                case K.Disaster: return 9;
                case K.Reinc: return 10;
                default: return 15;
            }
        }

        private IList<ClientCard> RegelWahl(IList<ClientCard> cards, int min, int max, long hint)
        {
            int n = Math.Max(1, min);
            // Enma-Schutz oder Rage/Escape mit vorgemerktem Ziel
            if (_ziel != null && cards.Contains(_ziel))
            {
                var z = _ziel; _ziel = null;
                return Util.CheckSelectCount(new List<ClientCard> { z }, cards, min, max);
            }
            // Nur Gegnerkarten: höchster Wert
            if (cards.All(c => c.Controller == 1))
                return cards.OrderByDescending(GegnerWert).Take(n).ToList();
            // End Phase von Enma: bis zu 2
            if (_aktivId == Enma && Duel.Phase == DuelPhase.End)
            {
                var liste = cards.Where(c => Art(c) != K.Abominator).OrderBy(c => EnmaZielWert(Art(c))).Take(Math.Max(n, Math.Min(max, 2))).ToList();
                return liste.Count > 0 ? liste : cards.Take(n).ToList();
            }
            // Handkarten abwerfen (Abominable Soul, Handlimit)
            if (cards.All(c => c.Location == CardLocation.Hand) && (hint == HintMsg.Discard || Duel.Phase == DuelPhase.End))
                return cards.OrderBy(c => AbwurfWert(Art(c))).Take(n).ToList();
            // Beschwören aus Hand/Deck/Friedhof
            if (cards.All(c => c.Location == CardLocation.Deck || c.Location == CardLocation.Hand || c.Location == CardLocation.Grave))
            {
                if (hint == HintMsg.AddToHand)
                    return cards.OrderBy(c => HandWert(Art(c))).Take(n).ToList();
#if !OHNE_SOULRAGE
                // Soul Lord aus dem Friedhof im Gegnerzug: Rage zurück, sie nimmt ein gegnerisches Monster als Material (Tims Guide 03.11.)
                if (_aktivId == SoulLord && GegnerMain && cards.Any(c => Is(c, Rage)) && Enemy.GetMonsters().Any(m => m != null && m.IsFaceup() && m.IsSpecialSummoned && !Unbeeinflussbar(m) && !ZerstoerSchutz.KeinMonsterZiel(m) && GegnerWert(m) >= 18))
                    return Util.CheckSelectCount(cards.Where(c => Is(c, Rage)).Take(1).ToList(), cards, min, max);
#endif
                if (cards.Any(c => Art(c) >= K.Prison && Art(c) <= K.Syncretism))
                    return cards.OrderBy(c => Array.IndexOf(new[] { K.Chamber, K.Escape, K.Syncretism, K.Reinc, K.Wailing }, Art(c)) is int w && w >= 0 ? w : 9).Take(n).ToList();
                return cards.OrderBy(c => SsPrioWert(Art(c))).Take(n).ToList();
            }
            // Eigene Karten zerstören: was noch etwas auslöst, sonst das Unwichtigste
            if (cards.Any(c => c.Controller == 0 && (c.Location == CardLocation.MonsterZone || c.Location == CardLocation.SpellZone)))
            {
                var gegner = cards.Where(c => c.Controller == 1 && !Unbeeinflussbar(c)).OrderByDescending(GegnerWert).ToList();
                if (gegner.Count > 0 && hint != HintMsg.LinkMaterial) return gegner.Take(n).ToList();
                return cards.OrderBy(EigenWert).Take(n).ToList();
            }
            return null;
        }
    }
}
