using System;
using System.Collections.Generic;
using System.Linq;
using YGOSharp.OCGWrapper.Enums;
using WindBot;
using WindBot.Game;

namespace WindBot.Game.AI.Decks
{
    /// <summary>Gemeinsam für alle KIs (Keto 03.11.: Zerstörungseffekte auf Monster, die nicht zerstört werden können, darf es bei keiner KI geben;
    /// Duell 16:48: Abominable Soul und Wailing verpufften an Borrelend). Die Tabelle kommt aus den Kartentexten (schutz_tabelle.py).
    /// Dazu lernt die KI im Zug: Karte als Zerstörungsziel gewählt, Kette vorbei, Karte steht noch → gilt bis Zugende als geschützt
    /// (Schutz durch andere Karten, Ersatzeffekte wie Enma). WindBot sieht aktive Schutzeffekte nicht.</summary>
    internal static partial class ZerstoerSchutz
    {
        internal const int AlsMonster = 1, AlsZauber = 2, AlsFalle = 4, Unbekannt = 0;

        private static readonly List<ClientCard> _gezielt = new List<ClientCard>();
        private static readonly HashSet<ClientCard> _gelernt = new HashSet<ClientCard>();

        internal static int Art(ClientCard quelle)
        {
            if (quelle == null) return Unbekannt;
            if (quelle.HasType(CardType.Monster)) return AlsMonster;
            if (quelle.HasType(CardType.Spell)) return AlsZauber;
            if (quelle.HasType(CardType.Trap)) return AlsFalle;
            return Unbekannt;
        }

        private static bool Trifft(byte schutz, int quelle) => quelle == Unbekannt ? schutz == 7 : (schutz & quelle) != 0;

        private static int Kennung(ClientCard c)
        {
            int alias = c.Data != null ? c.Data.Alias : 0;
            return alias != 0 ? alias : c.Id;
        }

        private static bool Wirkt(ClientCard c) => c != null && c.IsFaceup() && !c.IsDisabled()
            && (c.Location == CardLocation.MonsterZone || c.Location == CardLocation.SpellZone);

        /// <summary>Laut Kartentext immer geschützt (nicht negiert). Hartes Verbot für Zerstörungseffekte.</summary>
        internal static bool Sicher(ClientCard c, int quelle = Unbekannt)
            => Wirkt(c) && SicherDaten.TryGetValue(Kennung(c), out byte s) && Trifft(s, quelle);

        /// <summary>Wird diese Zerstörung sicher oder nach Erfahrung dieses Zuges verpuffen?</summary>
        internal static bool Verpufft(ClientCard c, int quelle = Unbekannt) => c != null && (Sicher(c, quelle) || _gelernt.Contains(c));

        /// <summary>Schutz unter Bedingung (z. B. Borrelcode, solange ein Link auf ihn zeigt): meiden, wenn es ein anderes Ziel gibt.</summary>
        internal static bool Meiden(ClientCard c, int quelle = Unbekannt)
            => Verpufft(c, quelle) || Wirkt(c) && BedingtDaten.TryGetValue(Kennung(c), out byte s) && Trifft(s, quelle);

        /// <summary>Monstereffekte können die Karte nicht als Ziel wählen (Borrelend): Der Planer darf sie nicht einplanen.</summary>
        internal static bool KeinMonsterZiel(ClientCard c) => Wirkt(c) && KeinZielDaten.Contains(Kennung(c));

        /// <summary>Offenes, nicht negiertes Monster, das laut Kartentext negieren kann.</summary>
        internal static bool Negierer(ClientCard c) => Wirkt(c) && NegiererDaten.Contains(Kennung(c));

        /// <summary>Gibt es unter den Karten mindestens ein Ziel, das die Zerstörung wirklich trifft?</summary>
        internal static bool HatZiel(IEnumerable<ClientCard> karten, int quelle = Unbekannt)
            => karten != null && karten.Any(c => c != null && !Verpufft(c, quelle));

        internal static IEnumerable<ClientCard> Treffbar(IEnumerable<ClientCard> karten, int quelle = Unbekannt)
            => karten.Where(c => c != null && !Verpufft(c, quelle));

        /// <summary>Divine Anima und Abyss Vagrada: unbeeinflusst von aktivierten Effekten von Monstern mit weniger ATK (Ketos Cubic-Duelle 04.11., Effekte verpufften an Anima).</summary>
        private static readonly HashSet<int> AtkVergleichDaten = new HashSet<int> { 100460004, 100460003 };

        /// <summary>Wirkt der Monstereffekt dieser Quelle gar nicht auf die Karte? Nur Monster als Quelle.</summary>
        internal static bool AtkImmun(ClientCard c, ClientCard quelleKarte)
            => quelleKarte != null && quelleKarte.HasType(CardType.Monster) && Wirkt(c) && AtkVergleichDaten.Contains(Kennung(c)) && quelleKarte.Attack < c.Attack;

        /// <summary>Am Ende jeder OnSelectCard-Auswahl: geschützte Gegnerkarten bei „Zerstören“ gegen treffbare tauschen und Ziele merken.
        /// gewaehlt == null ist WindBots Standardauswahl; mit Vorwahl (AI.SelectCard) bleibt sie, die prüft die Regel davor.</summary>
        internal static IList<ClientCard> Pruefe(IList<ClientCard> gewaehlt, IList<ClientCard> cards, int min, int max, long hint, bool vorwahl, ClientCard quelleKarte, Func<ClientCard, int> wert = null)
        {
            bool zerstoeren = hint == HintMsg.Destroy;
            if (!zerstoeren && hint != HintMsg.Negate && hint != HintMsg.Faceup) return gewaehlt;
            int q = Art(quelleKarte);
            int w(ClientCard c) => wert != null ? wert(c) : (c.Location == CardLocation.MonsterZone ? Math.Max(c.Attack, 1) : 500);
            bool problem(ClientCard c) => c.Controller == 1 && (zerstoeren && Meiden(c, q) || AtkImmun(c, quelleKarte));
            bool verpufft(ClientCard c) => zerstoeren && Verpufft(c, q) || AtkImmun(c, quelleKarte);
            if (gewaehlt == null)
            {
                if (vorwahl || !cards.Any(problem)) return null;
                var gut = cards.Where(c => c.Controller == 1 && !problem(c)).OrderByDescending(w).ToList();
                if (gut.Count == 0) gut = cards.Where(c => c.Controller == 1 && !verpufft(c)).OrderByDescending(w).ToList();
                if (gut.Count == 0 && cards.Any(c => c.Controller == 0)) return null;
                if (gut.Count == 0) gut = cards.ToList();
                gewaehlt = gut.Take(Math.Max(1, min)).ToList();
            }
            else if (gewaehlt.Any(problem))
            {
                var neu = gewaehlt.Where(c => !problem(c)).ToList();
                foreach (var ersatz in cards.Where(c => c.Controller == 1 && !neu.Contains(c) && !problem(c)).OrderByDescending(w))
                    if (neu.Count < gewaehlt.Count) neu.Add(ersatz);
                foreach (var ersatz in cards.Where(c => c.Controller == 1 && !neu.Contains(c) && !verpufft(c)).OrderByDescending(w))
                    if (neu.Count < gewaehlt.Count) neu.Add(ersatz);
                if (neu.Count < min) foreach (var rest in gewaehlt) if (neu.Count < min && !neu.Contains(rest)) neu.Add(rest);
                gewaehlt = neu;
            }
            if (zerstoeren)
                foreach (var c in gewaehlt) if (c.Controller == 1 && (c.Location == CardLocation.MonsterZone || c.Location == CardLocation.SpellZone)) _gezielt.Add(c);
            return gewaehlt;
        }

        /// <summary>In OnChainEnd aufrufen: Was Ziel einer eigenen Zerstörung war und noch offen auf dem Feld steht, ist bis Zugende geschützt.</summary>
        internal static void KettenEnde()
        {
            foreach (var c in _gezielt)
                if (c.Controller == 1 && c.IsFaceup() && (c.Location == CardLocation.MonsterZone || c.Location == CardLocation.SpellZone)) _gelernt.Add(c);
            _gezielt.Clear();
        }

        /// <summary>In OnNewTurn aufrufen.</summary>
        internal static void NeuerZug() { _gezielt.Clear(); _gelernt.Clear(); }
    }
}
