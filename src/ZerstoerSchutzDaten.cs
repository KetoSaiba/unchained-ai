// Erzeugt von schutz_tabelle.py aus den Kartentexten, nicht von Hand ändern.
using System.Collections.Generic;

namespace WindBot.Game.AI.Decks
{
    internal static partial class ZerstoerSchutz
    {
        /// <summary>Immer geschützt, solange der Effekt nicht negiert ist. Bits: 1 Monster, 2 Zauber, 4 Fallen.</summary>
        private static readonly Dictionary<int, byte> SicherDaten = new Dictionary<int, byte>
        {
            { 645794, 7 },  // Majespecter Toad - Ogama
            { 1164211, 6 },  // Shield of the Millennium Dynasty
            { 1340142, 7 },  // Varar, Vaalmonican Concord
            { 1566817, 1 },  // Cupid Serve
            { 2204038, 2 },  // Valkyrie Brunhilde
            { 5126490, 7 },  // Neos Wiseman
            { 5506791, 7 },  // Majespecter Cat - Nekomata
            { 5530780, 7 },  // Exosister Gibrine
            { 6616912, 7 },  // Gabrion, the Timelord
            { 6728559, 7 },  // Archnemeses Protos
            { 7020743, 7 },  // Tantrum Toddler
            { 7634581, 7 },  // Orichalcos Shunoros
            { 7733560, 7 },  // Michion, the Timelord
            { 8540986, 7 },  // Veidos the Dragon of Endless Darkness
            { 8763963, 7 },  // Beelzeus of the Diabolic Dragons
            { 9633505, 2 },  // Guardian Kay'est
            { 9634146, 7 },  // Signal Warrior
            { 9794980, 7 },  // Knightmare Incarnation Idlee
            { 10000090, 7 },  // The Winged Dragon of Ra - Immortal Phoenix
            { 10497636, 7 },  // War Rock Meteoragon
            { 10669138, 7 },  // Five-Headed Link Dragon
            { 10796448, 7 },  // Ace★Spades Speculation
            { 10920352, 7 },  // Masked HERO Vapor
            { 11224103, 2 },  // Horus the Black Flame Dragon LV6
            { 11443677, 4 },  // Blue-Eyes Tyrant Dragon
            { 11464648, 7 },  // Fiendsmith's Rextremende
            { 11738489, 7 },  // The Arrival Cyberse @Ignister
            { 12163590, 7 },  // Dragonmaid Cehrmba
            { 12600382, 6 },  // Exodia Necross
            { 12678870, 7 },  // Ritual Beast Ulti-Pettlephin
            { 13117073, 7 },  // Barricadeborg Blocker
            { 13173832, 7 },  // Salamangreat Wolvie
            { 13203964, 1 },  // Perfectron Hydradrive Dragon
            { 13331639, 7 },  // Supreme King Z-ARC
            { 13708888, 7 },  // Evil HERO Neos Lord
            { 15415552, 7 },  // Mortilux Heruvur
            { 15419596, 7 },  // Ghost Bird of Bewitchment
            { 17242022, 3 },  // Red-Eyes Black Dragon Exceed
            { 17242023, 3 },  // Red-Eyes Black Dragon Exceed
            { 17749468, 7 },  // Azamina
            { 17871506, 7 },  // Meteor Dragon Red-Eyes Impact
            { 18897163, 7 },  // D/D/D Super Doom King Dark Armageddon
            { 20654247, 7 },  // Blue-Eyes Chaos Dragon
            { 21377582, 7 },  // Master Peace, the True Dracoslaying King
            { 22638495, 7 },  // Dinoster Power, the Mighty Dracoslayer
            { 23219323, 1 },  // Junk Warrior/Assault Mode
            { 23220863, 1 },  // Magical Cavalry of Cxulub
            { 23689428, 7 },  // Infinitrack Goliath
            { 23776077, 7 },  // Hazy Flame Basiltrice
            { 23935886, 7 },  // Draco Masters of the Tenyi
            { 24514503, 7 },  // Soulicer Crystal
            { 24550676, 7 },  // Lunalight Leo Dancer
            { 25577965, 7 },  // Flametongue the Burning Blade
            { 26973555, 7 },  // Number F0: Utopic Future Dragon
            { 27240101, 7 },  // Kikinagashi Fucho
            { 28201945, 4 },  // Traptrix Nepenthes
            { 28373620, 7 },  // Geri the Runick Fangs
            { 28868394, 4 },  // Traptrix Vesiculo
            { 28929131, 7 },  // Zaphion, the Timelord
            { 29330706, 7 },  // Armed Protector Dragon
            { 30270176, 1 },  // Crimson Nova the Dark Cubic Lord
            { 30698243, 7 },  // Red Hypernova Dragon
            { 31178212, 7 },  // Majespecter Unicorn - Kirin
            { 31386180, 7 },  // Tiras, Keeper of Genesis
            { 31887806, 7 },  // Tobari the Sky Ninja
            { 31991800, 7 },  // Majespecter Raccoon - Bunbuku
            { 32061192, 7 },  // Maliss <P> Dormouse
            { 32216688, 7 },  // R.B. The Brute Blues
            { 32448765, 7 },  // Trickstar Holly Angel
            { 32448766, 7 },  // Trickstar Holly Angel
            { 32467459, 7 },  // El Shaddoll Meshachrer
            { 32588805, 7 },  // The Despair Uranus
            { 33015627, 7 },  // Sandaion, the Timelord
            { 33212663, 7 },  // Archnemeses Eschatos
            { 33955120, 7 },  // Rage Rose Witch
            { 34137269, 7 },  // Hailon, the Timelord
            { 34408491, 7 },  // Beelze of the Diabolic Dragons
            { 35057188, 7 },  // Battlewasp - Grand Partisan the Revolution
            { 36346532, 1 },  // Paleozoic Cambroraster
            { 36521459, 7 },  // Malefic Stardust Dragon
            { 37115973, 1 },  // Numeral Hunter
            { 37649320, 1 },  // Paleozoic Opabinia
            { 37818794, 7 },  // Red-Eyes Dark Dragoon
            { 38775407, 1 },  // Sengenjin Wakes from a Millennium
            { 40101111, 7 },  // Ultimate Axon Kicker
            { 40669071, 7 },  // Excode Talker
            { 41147577, 1 },  // CXyz Simon the Great Moral Leader
            { 41524885, 7 },  // Exosister Asophiel
            { 41589166, 4 },  // Tenkabito Shien
            { 42006475, 7 },  // Palladium Oracle Mana
            { 42632209, 7 },  // Geomathmech Final Sigma
            { 43803845, 7 },  // Duck Dummy
            { 44968687, 7 },  // The Legendary Fisherman III
            { 45420955, 7 },  // Grozer the Thunderclap Tyrant
            { 45803070, 4 },  // Traptrix Dionaea
            { 47172959, 7 },  // Yubel - The Loving Defender Forever
            { 47195442, 7 },  // Wonky Quartet
            { 47198668, 6 },  // D/D/D Doom King Armageddon
            { 47840168, 7 },  // Left-Hand Shark
            { 49027020, 4 },  // Traptrix Pudica
            { 50123605, 7 },  // Heavy Cavalry of the Indestructible Insects
            { 50939127, 6 },  // Different Dimension Dragon
            { 51788412, 6 },  // Chaos Ancient Gear Giant
            { 52698008, 7 },  // Cyberse Wicckid
            { 53451824, 7 },  // Mecha Phantom Beast Concoruda
            { 54082269, 7 },  // Blackwing Full Armor Master
            { 54701958, 7 },  // Lunalight Liger Dancer
            { 54919528, 7 },  // K9-ØØ "Hound"
            { 55410871, 7 },  // Blue-Eyes Chaos MAX Dragon
            { 55410872, 7 },  // Blue-Eyes Chaos MAX Dragon
            { 55428242, 4 },  // Traptrix Atrax
            { 55787576, 7 },  // World Legacy - "World Shield"
            { 55863245, 1 },  // Tatsunoko
            { 56733747, 7 },  // Elemental HERO Shining Neos Wingman
            { 57043117, 1 },  // Giant Soldier of Steel
            { 57761191, 7 },  // Metaltron XII, the True Dracombatant
            { 58518520, 7 },  // Clear Effector
            { 58604027, 7 },  // The Legendary Exodia Incarnate
            { 58699500, 7 },  // Cherubini, Ebon Angel of the Burning Abyss
            { 59120809, 1 },  // Scareclaw Tri-Heart
            { 59969392, 7 },  // Archfiend Zombie-Skull
            { 60181553, 7 },  // Mira the Star-Bearer
            { 60222213, 7 },  // Raphion, the Timelord
            { 61307542, 1 },  // Paleozoic Anomalocaris
            { 61307543, 1 },  // Paleozoic Anomalocaris
            { 61374414, 7 },  // CXyz N.As.Ch. Knight
            { 63612442, 4 },  // X-Saber Souza
            { 64276752, 7 },  // Arc Rebellion Xyz Dragon
            { 64591429, 7 },  // Astral Kuriboh
            { 65302903, 7 },  // Dominusteer Rosepamela
            { 65302904, 7 },  // Dominusteer Rosepamela
            { 65314286, 7 },  // Sadion, the Timelord
            { 65541655, 7 },  // Red Nova Dragon - Burning Soul
            { 65541656, 7 },  // Red Nova Dragon - Burning Soul
            { 65741786, 7 },  // I:P Masquerena
            { 65741787, 7 },  // I:P Masquerena
            { 65741789, 7 },  // I:P Masquerena
            { 67030233, 7 },  // Majestic Red Dragon
            { 67508932, 7 },  // Timelord Progenitor Vorpgate
            { 67547370, 7 },  // Zushin the Sleeping Giant
            { 68395509, 7 },  // Majespecter Crow - Yata
            { 68508433, 7 },  // Master Spirit Tech Force - Pendulum Ruler
            { 68987122, 7 },  // Borrelguard Dragon
            { 69073023, 1 },  // Infinitrack Fortress Megaclops
            { 70117860, 7 },  // Windwitch - Snow Bell
            { 70488851, 1 },  // Vidolium the Unstable Power Patron of Unity
            { 71222868, 7 },  // Raidraptor - Rising Rebellion Falcon
            { 71797713, 7 },  // Elementsaber Lapauila Mana
            { 71858682, 7 },  // Centur-Ion Auxila
            { 72144675, 4 },  // Troposphere
            { 72162751, 7 },  // Queen of the Blazing Domain
            { 72258771, 4 },  // Koa'ki Meiru Valafar
            { 72427512, 7 },  // Mimicking Man-Eater Bug
            { 72443568, 2 },  // Silent Magician LV8
            { 72443569, 2 },  // Silent Magician LV8
            { 72631243, 7 },  // Energy Bravery
            { 72664875, 7 },  // Crimson Nova Trinity the Dark Cubic Lord
            { 72880377, 7 },  // Dark Hole Dragon
            { 73121813, 7 },  // Tenyi Spirit - Mula Adhara
            { 74388798, 2 },  // Silent Swordsman LV5
            { 74530899, 7 },  // Metaion, the Timelord
            { 74889525, 7 },  // El Rey Conquista Eldlich
            { 75416738, 4 },  // Traptrix Arachnocampa
            { 75676192, 7 },  // F.A. Motorhome Transport
            { 76815942, 7 },  // Lyrilusc - Independent Nightingale
            { 77313225, 7 },  // Rciela, Sinister Soul of the White Forest
            { 77571454, 7 },  // Number 69: Heraldry Crest - Dark Matter Demolition
            { 79514956, 7 },  // Elegy the Melodious Diva
            { 80073414, 7 },  // Absolute Axon Kicker
            { 80773359, 7 },  // Assault Blackwing - Onimaru the Divine Thunder
            { 82627406, 7 },  // Kiwi Magician Girl
            { 82738277, 4 },  // Traptrix Mantis
            { 83257450, 7 },  // The Unstoppable Exodia Incarnate
            { 84433295, 7 },  // Quintet Magician
            { 84988419, 7 },  // Bloom Diva the Melodious Choir
            { 85771020, 7 },  // Asmo Token
            { 85908279, 7 },  // Invoked Cocytus
            { 86062400, 1 },  // Xyz Avenger
            { 86165817, 7 },  // Evil HERO Malicious Bane
            { 86188410, 4 },  // Elemental HERO Wildheart
            { 86221741, 7 },  // Raidraptor - Ultimate Falcon
            { 86926989, 7 },  // Shiranui Skillsaga Supremacy
            { 87054946, 7 },  // Nephthys, the Sacred Flame
            { 87182127, 7 },  // Ancient Gear Howitzer
            { 88078306, 4 },  // Traptrix Genlisea
            { 89194103, 7 },  // The Fabled Kudabbi
            { 89571015, 1 },  // Storm Cipher
            { 90590303, 7 },  // Number 41: Bagooska the Terribly Tired Tapir
            { 90590304, 7 },  // Number 41: Bagooska the Terribly Tired Tapir
            { 90884403, 7 },  // Phantasmal Lord Ultimitl Bishbaalkin
            { 91123920, 2 },  // The Agent of Force - Mars
            { 91712985, 7 },  // Kamion, the Timelord
            { 91812341, 4 },  // Traptrix Myrmeleo
            { 92435533, 7 },  // Lazion, the Timelord
            { 92770064, 7 },  // Sunavalon Dryatrentiay
            { 93449450, 1 },  // F.A. Hang On Mach
            { 94130731, 7 },  // Transcendosaurus Glaciasaurus
            { 94784213, 7 },  // Majespecter Fox - Kyubi
            { 94977269, 7 },  // El Shaddoll Winda
            { 94977270, 7 },  // El Shaddoll Winda
            { 95000026, 7 },  // Don Thousand/Monster C
            { 95000030, 7 },  // Z-ONE/Monster A
            { 95000031, 7 },  // Hairon, the Timelord Token
            { 95000032, 7 },  // Camion, the Timelord Token
            { 95000033, 7 },  // Z-ONE/Monster B
            { 95000034, 7 },  // Zadion, the Timelord Token
            { 95000035, 7 },  // Raphion, the Timelord Token
            { 95000036, 7 },  // Z-ONE/Monster D
            { 95000038, 7 },  // Z-ONE/Monster C
            { 95000039, 7 },  // Metaion, the Timelord Token
            { 95000040, 7 },  // Tzaphion, the Timelord Token
            { 95493471, 7 },  // Neo Super Quantal Mech King Blaster Magna
            { 96864105, 7 },  // Number C73: Abyss Supra Splash
            { 97165977, 7 },  // Lunalight Panther Dancer
            { 97489701, 7 },  // Red Nova Dragon
            { 97489702, 7 },  // Red Nova Dragon
            { 98630720, 7 },  // Borrelend Dragon
            { 99212922, 7 },  // White Dragon Ninja
            { 99585850, 7 },  // Red Supernova Dragon
            { 100000002, 7 },  // Sandaion, the Timelord (VG)
            { 100000003, 7 },  // Hailon, the Timelord (VG)
            { 100000004, 7 },  // Raphion, the Timelord (VG)
            { 100000005, 7 },  // Gabrion, the Timelord (VG)
            { 100000006, 7 },  // Kamion, the Timelord (VG)
            { 100000007, 7 },  // Zaphion, the Timelord (VG)
            { 100000008, 7 },  // Michion, the Timelord (VG)
            { 100000009, 7 },  // Lazion, the Timelord (VG)
            { 100000015, 7 },  // Sadion, the Timelord (VG)
            { 100000054, 7 },  // Wisel Carrier (TF6)
            { 100000110, 7 },  // Zushin the Sleeping Giant (VG)
            { 100000322, 7 },  // Dizzy Angel
            { 100000538, 2 },  // Valkyrie Brunhilde (Anime)
            { 100001010, 4 },  // Litmus Doom Swordsman (VG)
            { 100010001, 7 },  // New Orders 13 Etheric Amon
            { 100460051, 7 },  // Ghostrick Yoko
            { 101104016, 4 },  // Traptrix Ginalloa
            { 101402084, 7 },  // Kumenyo, the Nine-Hued Deer
            { 101403017, 7 },  // Invincible Rocket Warrior
            { 101403030, 7 },  // The Sun God of Darkness - The Winged Dragon of Ra
            { 101403035, 7 },  // Omega Vidolium the Power Patron of Heaven's End
            { 101403130, 7 },  // The Sun God of Darkness - The Winged Dragon of Ra
            { 140000083, 6 },  // Air Fortress Ziggurat
            { 160004022, 4 },  // Wyrm Excavator the Heavy Cavalry Draco
            { 160005015, 4 },  // Hyper Engine Vast Vulcan
            { 160012041, 7 },  // Dulkmua, the Phantom Skysavior
            { 160015022, 7 },  // Surging-Wave Swordsman Onimaru Kunituna
            { 160015045, 7 },  // Pitch-Dark Surging-Wave Swordsman Onimaru Kunituna
            { 160016036, 7 },  // Deity of Seven Treasures - Ryozai
            { 160016038, 7 },  // Praime Cat Straynya
            { 160017049, 7 },  // Dark Matter Enigknight Sieger
            { 160019027, 3 },  // I'll Love Ange
            { 160020029, 7 },  // Animagica Knight
            { 160020043, 7 },  // Galactica Chaos Oblivion
            { 160020046, 7 },  // Wheatcloth Demeter Hotpot
            { 160021000, 7 },  // Dark Magician the Dragon Knight (Rush)
            { 160021040, 7 },  // Meika Etraynze the Shadow Flower Fiery Flower Beauty
            { 160021045, 7 },  // Super Cervical Dragon Elasmortar
            { 160021048, 7 },  // Plasmatic Model Rising Blau
            { 160021140, 7 },  // Meika Etraynze the Shadow Flower Fiery Flower Beauty
            { 160022030, 7 },  // Tune Cornetless the Music Princess
            { 160023033, 7 },  // Rikuen the Molten Martial Machine
            { 160023039, 7 },  // Draco the Doubly Tiny
            { 160023042, 7 },  // Vic Viper Full Custom
            { 160024010, 7 },  // Emir of the Monumenthes
            { 160024032, 7 },  // Eternum Voidvelg Requiem
            { 160024037, 7 },  // Isyria, Deity of the Monumenthes
            { 160024137, 7 },  // Isyria, Deity of the Monumenthes
            { 160025023, 7 },  // Rise Motor Keelcopper
            { 160025070, 7 },  // Exalted Celestial Custodian of the Sacred Summit
            { 160025170, 7 },  // Exalted Celestial Custodian of the Sacred Summit
            { 160026008, 7 },  // The Sound of Piano Heard After School
            { 160026021, 7 },  // Healthie Lamilla
            { 160026032, 7 },  // The Sacred Shield of Arthenée
            { 160027024, 7 },  // Sinner of the Blightblaze
            { 160027038, 7 },  // Jointech Ultimate Warrior
            { 160027041, 7 },  // Zeil the King of the Hollowed Grounds
            { 160202002, 4 },  // Super Magitek Deity Magnum Over Road
            { 160202011, 4 },  // Yggdrago the Heavenly Emperor Dragon Tree
            { 160203007, 4 },  // Dynamic Dino Dynarmix [R]
            { 160205005, 4 },  // Supreme Machine Magnum Overlord
            { 160205007, 4 },  // Doomblaze Fiend Overlord Despairacion [L]
            { 160205011, 4 },  // Blaze Fiend Overlords Beelucitaroth
            { 160205052, 4 },  // Yamiruler the Dark Delayer - Supreme Soldier Spear
            { 160205107, 4 },  // Doomblaze Fiend Overlord Despairacion [L]
            { 160205152, 4 },  // Yamiruler the Dark Delayer - Supreme Soldier Spear
            { 160207005, 4 },  // Supreme Beasteam Magnum Overlord
            { 160208001, 4 },  // Harpie Lady Sisters [L]
            { 160208003, 1 },  // Harpie Lady Sisters [R]
            { 160213001, 7 },  // Red-Eyes Maxi-Flare Dragon [L]
            { 160213053, 7 },  // Fleeting Delirium Silbyx
            { 160217003, 7 },  // Valstorm the Skullcrag King
            { 160217032, 7 },  // Great Moth Alternative
            { 160218023, 7 },  // Lock Core the Mad Fiend of Despair
            { 160220002, 7 },  // Dark Magician the Dragon Knight (Rush)
            { 160322001, 6 },  // Harpie Lady Sisters Triangle Beauty
            { 160402068, 7 },  // Lostream Nereides
            { 160421006, 4 },  // Yggdrago the Sky Emperor
            { 160422002, 4 },  // Great Multistrike Dragon Dragias Burst
            { 160428002, 4 },  // Supreme Beasteam Magnum Overlord
            { 160452017, 7 },  // Deity of Seven Treasures - Ryozai
            { 160461030, 4 },  // Elemental HERO Wildheart (Rush)
            { 504700126, 2 },  // Silent Swordsman LV5 (GOAT)
            { 511000175, 2 },  // Necro Jar
            { 511000240, 7 },  // Exodia Necross (Anime)
            { 511000243, 7 },  // Exodius the Ultimate Forbidden Lord (Anime)
            { 511000261, 6 },  // Raviel, Lord of Phantasms (Anime)
            { 511000296, 7 },  // Number iC1000: Numeronius Numeronia (Anime)
            { 511000497, 7 },  // Neos Wiseman (Anime)
            { 511001372, 1 },  // Number C88: Gimmick Puppet Disaster Leo (Anime)
            { 511002009, 7 },  // Lunalight Panther Dancer (Anime)
            { 511002079, 7 },  // Raidraptor - Ultimate Falcon (Anime)
            { 511002195, 7 },  // Destiny End Dragoon (Anime)
            { 511002228, 7 },  // Frightfur Sabre-Tooth (Anime)
            { 511002358, 1 },  // Elemental HERO Ice Edge (Manga)
            { 511008508, 7 },  // Castle Gate (Manga)
            { 511009000, 7 },  // Timelord Progenitor Vorpgate (Manga)
            { 511009372, 7 },  // Master Spirit Tech Force - Pendulum Ruler
            { 511009593, 7 },  // D/D/D Super Doom King Purple Armageddon (Anime)
            { 511009680, 7 },  // Sunavalon Dryatrentiay (Anime)
            { 511009706, 7 },  // Borrelguard Dragon (Anime)
            { 511010053, 7 },  // Number 53: Heart-eartH (Anime)
            { 511010514, 7 },  // D/D/D Super Doom King Bright Armageddon (Anime)
            { 511010515, 7 },  // D/D/D Super Doom King Dark Armageddon (Anime)
            { 511010701, 2 },  // Different Dimension Dragon (Anime)
            { 511023015, 2 },  // Evil HERO Infernal Sniper (Anime)
            { 511600030, 6 },  // Obelisk The Progenitor
            { 511600342, 7 },  // Salamangreat Wolvie (Anime)
            { 511600398, 7 },  // Obelisk the Tormentor (Anime)
            { 511600399, 7 },  // Slifer the Sky Dragon (Anime)
            { 511600400, 7 },  // The Winged Dragon of Ra (Anime)
            { 511600405, 7 },  // Wisel Carrier (TF5)
            { 513000003, 7 },  // Zaphion, the Timelord (Anime)
            { 513000013, 7 },  // Red Nova Dragon (Anime)
            { 513000031, 1 },  // Shooting Quasar Dragon (Anime)
            { 513000039, 7 },  // Metaion, the Timelord (Anime)
            { 513000040, 7 },  // Kamion, the Timelord (Anime)
            { 513000041, 7 },  // Lazion, the Timelord (Anime)
            { 513000042, 7 },  // Hailon, the Timelord (Anime)
            { 513000043, 7 },  // Gabrion, the Timelord (Anime)
            { 513000044, 7 },  // Sadion, the Timelord (Anime)
            { 513000045, 7 },  // Michion, the Timelord (Anime)
            { 513000049, 7 },  // Sandaion, the Timelord (Anime)
            { 513000050, 7 },  // Raphion, the Timelord (Anime)
            { 513000134, 7 },  // The Winged Dragon of Ra (Manga)
            { 513000135, 6 },  // Obelisk the Tormentor (Manga)
            { 513000136, 6 },  // Slifer the Sky Dragon (Manga)
            { 513000137, 6 },  // The Wicked Dreadroot (Manga)
            { 513000138, 6 },  // The Wicked Avatar (Manga)
            { 513000139, 6 },  // The Wicked Eraser (Manga)
            { 513000141, 7 },  // Number 69: Heraldry Crest (Anime)
            { 513000171, 7 },  // Wisel Carrier (Anime)
        };
        /// <summary>Geschützt unter einer Bedingung im Kartentext: nur nehmen, wenn es nichts Besseres gibt.</summary>
        private static readonly Dictionary<int, byte> BedingtDaten = new Dictionary<int, byte>
        {
            { 1035143, 7 },  // Darkuriboh
            { 1872843, 7 },  // Morpheus, the Dream Mirror White Knight
            { 2254222, 7 },  // Speedroid Wing Synchron
            { 3134857, 7 },  // Orcustrion
            { 3149401, 7 },  // Ultimate Dragon of Pride and Soul
            { 3611830, 7 },  // Endymion, the Mighty Master of Magic
            { 4417407, 7 },  // Mecha Phantom Beast Blackfalcon
            { 4786063, 7 },  // Superheavy Samurai Soulfire Suit
            { 6075801, 1 },  // Paladin of Felgrand
            { 6511113, 4 },  // Traptrix Rafflesia
            { 6659193, 7 },  // Performage Trapeze High Magician
            { 8165596, 7 },  // Number 90: Galaxy-Eyes Photon Lord
            { 9485511, 7 },  // U.A. Goalkeeper
            { 9822220, 7 },  // Lord of the Heavenly Prison
            { 12067160, 1 },  // Gorgon of Zilofthonia
            { 12890860, 7 },  // Denial Deity Dotan
            { 13021682, 7 },  // Starjunk Synchron
            { 13021683, 7 },  // Starjunk Synchron
            { 13518809, 7 },  // Ruin, Supreme Queen of Oblivion
            { 13760677, 7 },  // P.M. Captor
            { 13836592, 1 },  // Behemoth the King of a Hundred Battles
            { 15005145, 7 },  // Centur-Ion Primera
            { 15335853, 7 },  // Mecha Phantom Beast Sabre Hawk
            { 15495787, 7 },  // Superheavy Samurai Prepped Defense
            { 15744417, 7 },  // Orgoth the Relentless
            { 15848542, 7 },  // Dragonmaid Lorpar
            { 15914410, 7 },  // Mechquipped Angineer
            { 16617334, 7 },  // Performapal Rain Goat
            { 16643334, 7 },  // Starliege Photon Blast Dragon
            { 16922142, 6 },  // Radiant Typhoon Krosea
            { 16943770, 7 },  // Mecha Phantom Beast Aerosguin
            { 18511599, 7 },  // Evolzar Solda
            { 18585765, 7 },  // Voltester
            { 18861006, 7 },  // Crimson Firewing Pegasus
            { 19333131, 7 },  // Number 12: Crimson Shadow Armor Ninja
            { 20368763, 7 },  // Mecha Phantom Beast Harrliard
            { 21293424, 7 },  // Gunkan Suship Shirauo-class Carrier
            { 21351206, 7 },  // Infernoble Knight Ogier
            { 22110647, 7 },  // Mecha Phantom Beast Dracossack
            { 22510667, 7 },  // Gouki The Solid Ogre
            { 23085002, 7 },  // Number 68: Sanaphond the Sky Prison
            { 23187256, 7 },  // Number 93: Utopia Kaiser
            { 28798938, 7 },  // Dual Avatar Lion Envoys - Ah-Un
            { 28904860, 7 },  // Angelechy Bastion
            { 29302858, 7 },  // Vanquish Soul Razen
            { 30811116, 7 },  // Mecha Phantom Beast Stealthray
            { 31480215, 7 },  // Mecha Phantom Beast Warbluran
            { 31533704, 7 },  // Mecha Phantom Beast Megaraptor
            { 31539614, 7 },  // Antidote Nurse
            { 32138660, 7 },  // Reptilianne Melusine
            { 33545259, 7 },  // Ancient Warriors - Ambitious Cao De
            { 34031284, 7 },  // Ojama Emperor
            { 34198387, 7 },  // Meowseclick
            { 34379489, 7 },  // Performapal Bubblebowwow
            { 35252119, 7 },  // Virtual World Beast - Jiujiu
            { 36322312, 7 },  // Boot-Up Admiral - Destroyer Dynamo
            { 37006702, 7 },  // Krishnerd Witch
            { 37433748, 7 },  // SPYRAL GEAR - Last Resort
            { 38229962, 7 },  // Giant Beetrooper Invincible Atlas
            { 38502358, 7 },  // Mekk-Knight Spectrum Supreme
            { 39016067, 7 },  // Wind Unicorn Parallel, the Dracoslayer
            { 39972129, 7 },  // Number 64: Ronin Raccoon Sandayu
            { 40908371, 7 },  // Azure-Eyes Silver Dragon
            { 40945356, 7 },  // Twilight Ninja Nichirin, the Chunin
            { 41090784, 6 },  // Dai-sojo of the Ice Barrier
            { 41329458, 7 },  // Mecha Phantom Beast Kalgriffin
            { 41456841, 7 },  // Metamorphosed Insect Queen
            { 41522092, 7 },  // Number F0: Utopic Future Zexal
            { 42055234, 7 },  // Dragonmaid Tinkhec
            { 43268675, 7 },  // Opera the Melodious Diva
            { 43730887, 7 },  // Holding Arms
            { 43886072, 7 },  // Prank-Kids Bow-Wow-Bark
            { 44001993, 7 },  // Magician of Dark Chaos - Black Chaos
            { 44001994, 7 },  // Magician of Dark Chaos - Black Chaos
            { 44026393, 7 },  // Mecha Phantom Beast Raiten
            { 44364207, 4 },  // Jade Knight
            { 44405066, 7 },  // Red-Eyes Flare Metal Dragon
            { 45082499, 7 },  // ZW - Lightning Blade
            { 45955628, 7 },  // Catoblepas, Familiar of the Evil Eye
            { 46132282, 7 },  // Powered Inzektron
            { 48469380, 1 },  // Archfiend Emperor
            { 48608796, 7 },  // Lyrilusc - Assembled Nightingale
            { 49202162, 7 },  // Black Luster Soldier - Soldier of Chaos
            { 49575521, 7 },  // Dragonmaid Nudyarl
            { 51073802, 7 },  // Majespecter Porcupine - Yamarashi
            { 51566770, 7 },  // Infernity Guardian
            { 51849482, 7 },  // Finis Terrae, Tower of the Necroworld
            { 52119435, 7 },  // Geonator Transverser
            { 53413628, 7 },  // Code Talker
            { 53589300, 7 },  // Nerva the Power Patron of Creation
            { 53842431, 2 },  // Mythical Beast Master Cerberus
            { 55537983, 7 },  // Mimighoul Master
            { 56051086, 7 },  // Number 43: Manipulator of Souls
            { 56198785, 7 },  // Guard Ghost
            { 56768355, 6 },  // Vylon Alpha
            { 57111330, 7 },  // Therion "Duke" Yul
            { 57541158, 7 },  // Marincess Sleepy Maiden
            { 58139128, 7 },  // Gravekeeper's Shaman
            { 58931850, 7 },  // Dragon Master Lords
            { 59251766, 7 },  // Bujingi Hare
            { 59281922, 7 },  // Cyber Dragon Drei
            { 60303245, 7 },  // Salamangreat Almiraj
            { 61272280, 6 },  // Vahram, the Magistus Divinity Dragon
            { 61757117, 7 },  // Divine Grace - Northwemko
            { 61888819, 7 },  // Steelswarm Origin
            { 66200210, 7 },  // Mecha Phantom Beast Hamstrat
            { 67288539, 7 },  // Borrelcode Dragon
            { 67489919, 7 },  // Mecha Phantom Beast Blue Impala
            { 67712104, 1 },  // Marincess Crystal Heart
            { 67808837, 6 },  // Performapal King Bear
            { 67809530, 7 },  // The Crimson King
            { 67922702, 7 },  // Mecha Phantom Beast Tetherwolf
            { 68473226, 7 },  // Hardened Armed Dragon
            { 68823957, 7 },  // Subterror Nemesis Defender
            { 69058960, 7 },  // Number 13: Embodiment of Crime
            { 70659412, 7 },  // Psychic Omnibuster
            { 71108540, 7 },  // Invincible Demise Lord
            { 71209500, 7 },  // Amazoness Scouts
            { 71222649, 7 },  // Twilight Specter Rayrage
            { 71768839, 7 },  // I.A.S. -Invasive Alien Species-
            { 72171665, 7 },  // Magical Broker
            { 72246674, 7 },  // Gladiator Beast Dareios
            { 72291078, 7 },  // Mecha Phantom Beast O-Lion
            { 72318602, 7 },  // Red Gardna
            { 73167098, 7 },  // Aromalilith Magnolia
            { 74122412, 7 },  // Nekroz of Gungnir
            { 74163487, 7 },  // Restoration Point Guard
            { 75083197, 7 },  // Dragonlark Pairen
            { 75452921, 7 },  // Knightmare Cerberus
            { 76067258, 7 },  // Number 66: Master Key Beetle
            { 76145142, 7 },  // Longirsu, the Orcust Orchestrator
            { 76782778, 7 },  // Dragonmaid Ernus
            { 76902476, 7 },  // Mecha Phantom Beast Turtletracer
            { 77087109, 7 },  // Clock Resonator
            { 77610772, 7 },  // Ib the World Chalice Priestess
            { 78744660, 1 },  // Archfiend Matriarch
            { 79206925, 7 },  // Gravekeeper's Supernaturalist
            { 80141055, 7 },  // Bearblocker
            { 80555062, 7 },  // Bujingi Warg
            { 80889750, 7 },  // Frightfur Sabre-Tooth
            { 81035362, 7 },  // Fiendish Rhino Warrior
            { 81105204, 6 },  // Blackwing - Kris the Crack of Dawn
            { 81237046, 7 },  // Enneacraft - Archa.TAIL
            { 81497285, 7 },  // Lady Labrynth of the Silver Castle
            { 81497286, 7 },  // Lady Labrynth of the Silver Castle
            { 82315403, 7 },  // Cyber Eternity Dragon
            { 83994433, 7 },  // Stardust Spark Dragon
            { 84472026, 7 },  // Ghostrick Yeti
            { 84900597, 7 },  // Lamia
            { 85080444, 7 },  // Artifact Aegis
            { 86605515, 7 },  // Qardan the Clear-Sighted
            { 87564935, 1 },  // Metallizing Parasite - Soltite
            { 88757791, 7 },  // Dharma-Eye Magician
            { 92332424, 7 },  // Majesty Pegasus, the Dracoslayer
            { 93302695, 7 },  // Kozmoll Wickedwitch
            { 93730230, 7 },  // Chronomaly Crystal Chrononaut
            { 94973028, 7 },  // Mecha Phantom Beast Coltwing
            { 95218695, 7 },  // Subterror Behemoth Dragossuary
            { 95440946, 7 },  // Eldlich the Golden Lord
            { 95440947, 7 },  // Eldlich the Golden Lord
            { 95442074, 7 },  // Number 31: Embodiment of Punishment
            { 95937545, 7 },  // Golden Allure Queen
            { 96051150, 7 },  // Hazy Flame Mantikor
            { 96150936, 7 },  // Madolche Fresh Sistart
            { 97522863, 7 },  // The Man with the Mark
            { 99217226, 7 },  // Paladins of Bonds and Unity
            { 100430016, 7 },  // Vanquish Soul - Razen
            { 100458004, 7 },  // Tauzenet Wadjet
            { 100459004, 7 },  // Blue-Eyes Ultimate Dragon, the God-Surpassing Servant
            { 100459013, 7 },  // Evil Dragon Cards - Earth Dragon
            { 100459018, 7 },  // King of Yamimado
            { 101402101, 7 },  // Audhumla, Progenitor of the Frozen Expanse
            { 160001031, 7 },  // Defensive Dragon Mage
            { 160003037, 4 },  // Long Shield Gardna
            { 160004016, 4 },  // Insurrection Dragon
            { 160006017, 4 },  // Steel Strike Dragon Metagias
            { 160009015, 4 },  // Ozone Slayer
            { 160011034, 7 },  // Watchman of the Apocalypse
            { 160011035, 4 },  // Neo Plandish
            { 160013006, 7 },  // Voidvelg Kyrie
            { 160013035, 4 },  // Eternal Galactica Oblivion
            { 160013039, 7 },  // Voidvelg Theogonia
            { 160013043, 7 },  // Sevens Chariot
            { 160014004, 7 },  // Mega Jointech Fortrex
            { 160014016, 4 },  // Roaring Firestorm Volcalize Phoenix
            { 160014023, 7 },  // Abysslayer Leviaknight
            { 160015034, 7 },  // Glacies the Snowmeister of Sacred Splendor
            { 160015047, 7 },  // Meteor Black Drake
            { 160016029, 4 },  // Jinzo - Amplified
            { 160016039, 7 },  // Transam Praime Full Armor Nova
            { 160016044, 7 },  // Celeb Rose Influencers
            { 160017048, 7 },  // Dark Matter Enigmata
            { 160017051, 7 },  // Tribemirror Dragon
            { 160018008, 7 },  // Hybridrive Barriertail
            { 160018022, 6 },  // Freshberry Happiness Harvest
            { 160018029, 7 },  // Meini the Defiant Soul
            { 160019007, 7 },  // Dice Key Mix
            { 160019008, 7 },  // Dice Key Retra
            { 160019019, 7 },  // Ancient Gear Gadjiltron Baena
            { 160019045, 7 },  // Dark Matter Galaxy Dragon
            { 160020008, 7 },  // Galactica Oblivion Ark
            { 160021039, 7 },  // Tamabot King Golem
            { 160022008, 4 },  // OuTerverSe Burst Oblivion
            { 160027019, 7 },  // Mad Rare Gramma
            { 160027043, 1 },  // Saintick Lumina
            { 160204033, 4 },  // Crescent Moon Swordmaster
            { 160205023, 1 },  // Cyber Stealth Dragon
            { 160205025, 4 },  // Cyber Gryphon
            { 160205047, 7 },  // Voidvelg Apocalypse
            { 160205049, 4 },  // Destroyer of Dragon Sorcerers
            { 160205147, 7 },  // Voidvelg Apocalypse
            { 160205149, 4 },  // Destroyer of Dragon Sorcerers
            { 160206035, 4 },  // Rogue of Archfiend
            { 160207002, 7 },  // Supreme Railartillery Magnum Overlord
            { 160207014, 7 },  // Abyssal Dragon Lord Abyss Poseidra
            { 160207017, 7 },  // Abyssal Sea Dragon Abyss Kraken
            { 160208018, 4 },  // Divinebreath the Orchestral Music Fiend
            { 160209080, 7 },  // Dark Sword Magician
            { 160210040, 7 },  // Abysskite Miracle Girls
            { 160210042, 7 },  // Conduction Warrior Alchemicalizer Suirai
            { 160211007, 7 },  // Deep Space Yggdrago
            { 160211014, 7 },  // Guardian of Cyberse
            { 160211024, 7 },  // Valkyrion the Magna Wings
            { 160211062, 7 },  // Sevens Road Charmy Witch
            { 160211069, 7 },  // Supreme Machine Burning Overlord
            { 160213033, 7 },  // Dynamail Dino Dynamix [R]
            { 160214002, 2 },  // Dark Paladin (Rush)
            { 160214025, 7 },  // Sevens Knight the Multistrike Dragon Knight
            { 160214028, 7 },  // Dark Cavalry (Rush)
            { 160215014, 7 },  // Ever-Grace Princess Mai
            { 160215041, 7 },  // Celeb Rose Influencers
            { 160215057, 4 },  // Divinebreath the Orchestral Music Fiend
            { 160215114, 7 },  // Ever-Grace Princess Mai
            { 160216007, 7 },  // Super Princess of the Flock - Max Mutton [L]
            { 160216058, 7 },  // Wish of OuTerverSe the World-Shattering Emperor [L]
            { 160217001, 7 },  // Gaigustvelgr the Defiant Soulclad Sword
            { 160220022, 7 },  // Waso Gozen Linker
            { 160220035, 7 },  // Cross Lostream Verdias
            { 160220061, 7 },  // Legendary Party
            { 160304003, 4 },  // Sportsdragon Closer
            { 160317001, 4 },  // Voidvelg Elysium
            { 160401016, 7 },  // Dark Matter Enigmata
            { 160401018, 7 },  // Galactica Oblivion Ark
            { 160401019, 4 },  // Eternal Galactica Oblivion
            { 160402032, 7 },  // Straynge Magician
            { 160409006, 4 },  // Transam Valuable Linac
            { 160428004, 4 },  // Solid Strike Dragon Metagiastar F
            { 160428024, 4 },  // Omega Guitarna the Supreme Shining Superstar
            { 160441003, 4 },  // Eternal Galactica Oblivion
            { 160452014, 4 },  // Voidvelg Elysium
            { 160452028, 7 },  // Sevens Road Charmy Witch
            { 160459034, 2 },  // Dark Paladin (Rush)
            { 160461013, 7 },  // Cosmostar the Hypernova Polairy
            { 511000192, 7 },  // Number F0: Utopic Future (Anime)
            { 511000234, 6 },  // Uria, Lord of Searing Flames (Anime)
            { 511000246, 6 },  // Hamon, Lord of Striking Thunder (Anime)
            { 511001489, 7 },  // The Legendary Fisherman III (Anime)
            { 511001776, 7 },  // Number 43: Manipulator of Souls (Anime)
            { 511002749, 7 },  // Number 66: Master Key Beetle (Anime)
            { 511002870, 7 },  // Number 102: Star Seraph Sentry (Anime)
            { 511002880, 7 },  // Clock Resonator (Anime)
            { 511003045, 7 },  // Performapal Odd-Eyes Synchron (Anime)
            { 511003213, 7 },  // Infernity Patriarch (Manga)
            { 511009018, 7 },  // Gouki The Solid Ogre (Anime)
            { 511009382, 7 },  // C/C/C Sonic Halberd of Battle
            { 511009514, 7 },  // Brutal Red
            { 511009519, 7 },  // Superheavy Samurai Warchief Heavystrong
            { 511009560, 7 },  // Performage Trapeze High Magician (Anime)
            { 511009632, 7 },  // Speedroid Marble Machine (Manga)
            { 511009683, 7 },  // Gouki Ringtrainer (Anime)
            { 511009735, 7 },  // Topologic Gumblar Dragon (Anime)
            { 511010083, 7 },  // Number 83: Galaxy Queen (Anime)
            { 511013000, 7 },  // Red Gardna (Anime)
            { 511018021, 7 },  // Timegazer Magician (Pre-Pendulum)
            { 511030070, 7 },  // Armatos Legio Magica Alcum
            { 511600072, 7 },  // Fault Tolerlancer
            { 511600091, 7 },  // Restoration Point Guard (Anime)
            { 511600125, 7 },  // Performapal Changeraffe (Anime)
            { 511600334, 7 },  // Salamangreat Lemming
            { 810000001, 7 },  // Invincible Demise Lord (Anime)
        };
        /// <summary>Kann nicht als Ziel von Monstereffekten gewählt werden, unabhängig von Bedingungen im Spiel.</summary>
        private static readonly HashSet<int> KeinZielDaten = new HashSet<int>
        {
            1409474,  // Hazy Flame Sphynx
            6165656,  // Number C88: Gimmick Puppet Disaster Leo
            8696773,  // Hazy Flame Hydra
            10000080,  // The Winged Dragon of Ra - Sphere Mode
            10136446,  // Darklord Eveningstar
            13331639,  // Supreme King Z-ARC
            15419596,  // Ghost Bird of Bewitchment
            16259549,  // Number 49: Fortune Tune
            20654247,  // Blue-Eyes Chaos Dragon
            24550676,  // Lunalight Leo Dancer
            28400508,  // Number 97: Draglubion
            30698243,  // Red Hypernova Dragon
            31303283,  // Hazy Flame Hyppogrif
            34550857,  // Lyrilusc - Cobalt Sparrow
            34695290,  // Myutant Beast
            37803172,  // Hazy Flame Peryton
            38525760,  // Hazy Flame Cerbereus
            38811586,  // Albion the Incandescent Dragon
            45420955,  // Grozer the Thunderclap Tyrant
            49394035,  // Vendread Core
            52159691,  // Raiders' Wing
            55410871,  // Blue-Eyes Chaos MAX Dragon
            55410872,  // Blue-Eyes Chaos MAX Dragon
            55885348,  // Kozmo Dark Destroyer
            69815951,  // Drytron Meteonis Draconids
            73121813,  // Tenyi Spirit - Mula Adhara
            74010769,  // Hazy Flame Griffin
            75191028,  // GMX - COMPREX
            82103466,  // Divine Serpent Geh
            85908279,  // Invoked Cocytus
            88753594,  // Lunalight Sabre Dancer
            88754763,  // CXyz Coach Lord Ultimatrainer
            91025875,  // K9-ØØ Lupis
            94641726,  // Storm-Bane Dragon Destorbim
            96051150,  // Hazy Flame Mantikor
            110000010,  // The Winged Dragon of Ra (VG)
            110000011,  // Obelisk the Tormentor (VG)
            110000012,  // Slifer the Sky Dragon (VG)
        };
        /// <summary>Monster, die laut Kartentext im Gegnerzug negieren können: als Zweiter zuerst ausschalten.</summary>
        private static readonly HashSet<int> NegiererDaten = new HashSet<int>
        {
            132308,  // Rikka Princess
            146746,  // Double Headed Anger Knuckle
            613013,  // Veiss Shark
            1174075,  // Drytron Mu Beta Fafnir
            1340142,  // Varar, Vaalmonican Concord
            1508649,  // Altergeist Hexstia
            1527418,  // Wiz, Sage Fur Hire
            1621413,  // Dark Requiem Xyz Dragon
            1697104,  // PSY-Framegear Epsilon
            1861629,  // Decode Talker
            1861630,  // Decode Talker
            1861631,  // Decode Talker
            1992816,  // Number 9: Dyson Sphere
            2311090,  // Spright Carrot
            2563463,  // Wandering Gryphon Rider
            2609443,  // Chronomaly Vimana
            3064425,  // Superheavy Samurai Soulbang Cannon
            3292267,  // Hieratic Heavenly Dragon Overlord of Heliopolis
            3611830,  // Endymion, the Mighty Master of Magic
            4026187,  // Teller of Fairy Tails
            4063756,  // Medicurius the Power Patron of Illusions
            4145852,  // Zoodiac Ramram
            4280258,  // Apollousa, Bow of the Goddess
            4280259,  // Apollousa, Bow of the Goddess
            4367330,  // Zoodiac Bunnyblast
            4628897,  // Imperion Magnum the Superconductive Battlebot
            4810828,  // Sauravis, the Ancient and Ascended
            4891376,  // Zalen the Shackled Dragon
            4906301,  // Necro Gardna
            4918855,  // Draculea Vampire
            4993187,  // W:P Fancy Ball
            5530780,  // Exosister Gibrine
            5818294,  // Bujingi Turtle
            5914858,  // Junora the Power Patron of Tuning
            6083904,  // Diabrocken the Ominous Specter
            6150044,  // Arcana Knight Joker
            6150045,  // Arcana Knight Joker
            6182103,  // Myutant Ultimus
            6568731,  // Glacial Beast Iceberg Narwhal
            6906306,  // Rubysapphirus, the Adamant Jewel
            7375867,  // Dracotail Mululu
            7628844,  // Number C32: Shark Drake LeVeiss
            7631534,  // Dual Avatar Feet - Armored Un-Gyo
            7841112,  // Majestic Star Dragon
            7894706,  // The Chaotic Phantasmal Sacred Beasts
            7894707,  // The Chaotic Phantasmal Sacred Beasts
            7987191,  // Borreload Riot Dragon
            8165596,  // Number 90: Galaxy-Eyes Photon Lord
            8315896,  // Candelato, the Beast of Light
            8505920,  // Gate Guardians Combined
            8836329,  // Raging Storm Dragon - Beaufort IX
            8963089,  // Heosvarog the Mechanical Dawn
            9349094,  // Divine Treebeast High Peryton
            9464441,  // Adamancipator Risen - Dragite
            9753964,  // Hot Red Dragon Archfiend Abyss
            10300821,  // Number C79: Battlin' Boxer General Kaiser
            10443957,  // Cyber Dragon Infinity
            10443958,  // Cyber Dragon Infinity
            10497636,  // War Rock Meteoragon
            10552026,  // Gouki Heel Ogre
            10604644,  // Therion "King" Regulus
            10673071,  // Virakam, the Divine Spirit of Artifice
            10774240,  // Skull Guardian, Protector of the Voiceless Voice
            11441009,  // P.U.N.K. JAM FEVER!
            11819616,  // Tytannial, Princess of Camellias
            12018201,  // Lavalval Exlord
            12219047,  // The Phantom Knights of Cursed Javelin
            12266229,  // Illusion of Chaos
            12298909,  // Prime Material Dragon
            12381100,  // Dragon Master Magia
            12800564,  // Master Peace, the True Dracoverlord
            13026402,  // A-Team: Trap Disposal Unit
            13482075,  // Vendread Chimera
            14152862,  // Norito the Moral Leader
            14309486,  // Koa'ki Meiru Overload
            14556954,  // VIP Whale
            14558127,  // Ash Blossom & Joyous Spring
            14558128,  // Ash Blossom & Joyous Spring
            15613529,  // Mystical Elf - White Lightning
            15665977,  // Kewl Tune RS
            15844566,  // Powercode Talker
            16006416,  // D/D/D Flame High King Genghis
            16037007,  // Number 74: Master of Blades
            17266660,  // Herald of Orange Light
            17739335,  // Zerrziel, Ruler of the Evil Eyed
            17946349,  // Mathmech Diameter
            18214905,  // Elementsaber Lapauila
            18621798,  // Fire King Avatar Arvata
            18832779,  // Plunder Patrollship Lys
            18847598,  // Advanced Crystal Beast Amber Mammoth
            18964575,  // Swift Scarecrow
            18967507,  // Cyberdarkness Dragon
            19096726,  // Tri-Brigade Mercourier
            19652159,  // Light and Darkness Dragonlord
            20155904,  // Zoodiac Kataroost
            20934852,  // Marincess Aqua Argonaut
            21074344,  // Herald of Green Light
            21123811,  // Cosmic Blazar Dragon
            21140872,  // Red-Eyes Slash Dragon
            21419436,  // Unchained Soul Rage Abominator
            21522601,  // Witchcrafter Madame Verre
            21522602,  // Witchcrafter Madame Verre
            21522603,  // Witchcrafter Madame Verre
            22091647,  // Immortal Phoenix Gearfried
            22586618,  // Simorgh of Darkness
            22790910,  // Wisdom Magus - Himmel
            22938501,  // Regenesis Sage
            23440231,  // Infernoid Devyaty
            23530726,  // Surgical Striker - S.P.E.C.T.R.A.
            23738096,  // Supply Soldier of the Indestructible Insects
            24027078,  // Morgan, the Enchantress of Avalon
            24203749,  // The Great Gallant Bandit
            24203750,  // The Great Gallant Bandit
            24696097,  // Shooting Star Dragon
            24779554,  // Rucia of the White Forest
            24799107,  // Dragonmaid Sheou
            24842059,  // Linguriboh
            25726386,  // Megalith Aratron
            26084285,  // Gravekeeper's Watcher
            26268488,  // Stardust Sifr Divine Dragon
            26973555,  // Number F0: Utopic Future Dragon
            27118421,  // Starving Venom Fusion Dragon of the Four Heavenly Dragons
            27118422,  // Starving Venom Fusion Dragon of the Four Heavenly Dragons
            27184601,  // Artmage Diactorus
            27346636,  // Gladiator Beast Heraklinos
            27354732,  // Mythical Beast Jackal King
            27420823,  // K9-17 "Ripper"
            27420824,  // K9-17 "Ripper"
            27548199,  // Borreload Savage Dragon
            27572350,  // Bystial Dis Pater
            27660735,  // Speedroid Tri-Eyed Dice
            27918365,  // World Legacy - "World Crown"
            27978707,  // Superheavy Samurai Flutist
            28331069,  // Neo Galaxy-Eyes Photon Dragon - Photon Howling
            28331070,  // Neo Galaxy-Eyes Photon Dragon - Photon Howling
            28388927,  // Battlewasp - Sting the Poison
            28454232,  // Enneacraft - Asta.PIXEA
            28954097,  // Mirror Swordknight
            29053656,  // Crimson Dragon Quetzacoatl
            29053657,  // Crimson Dragon Quetzacoatl
            29357687,  // Gorgon, Empress of the Evil Eyed
            29570824,  // Ekto Enneacraft - "tromarIA"
            29587993,  // Mist Valley Apex Avian
            29981921,  // Legendary Six Samurai - Shi En
            30221870,  // Meklord Emperor Wisel - Synchro Absorption
            30350202,  // Eclipse, Dragon Ruler of Catastrophes
            30397786,  // Blue-Eyes White Dragon, the White Phantom Beast
            30397787,  // Blue-Eyes White Dragon, the White Phantom Beast
            30998403,  // D/D/D Sky King Zeus Ragnarok
            31286915,  // K9-X "Ripper/M"
            32422602,  // Ancient Warriors - Ingenious Zhuge Kong
            32617464,  // Tri-Gate Wizard
            32965616,  // Elemental Grace Doriado
            33026283,  // Dual Avatar - Empowered Kon-Gyo
            33129626,  // Centaur Mina
            33158448,  // F.A. Dawn Dragster
            33171768,  // Axon Kicker Oracle
            33198837,  // Naturia Beast
            33302589,  // Constellar Castor
            33438265,  // R.B. Ga10 Cutter
            33652635,  // Gladiator Beast Domitianus
            33837653,  // Tenma the Sky Star
            34072799,  // Transonic Bird
            34079868,  // Master Peace, the True Dracoslayer
            34235530,  // Legendary Lord Six Samurai - Shi En
            34446231,  // Infernoid Flood
            34528176,  // Dragunity Knight - Areus
            34541940,  // Artmage Finmel
            34614289,  // U.A. Blockbacker
            34695290,  // Myutant Beast
            34976176,  // Blackwing - Chinook the Snow Blast
            35103106,  // Evolzar Lars
            35263180,  // Seed-Spitting Saplings
            35334193,  // Security Force Justify
            35763582,  // Lunalight Silver Hound
            35952884,  // Shooting Quasar Dragon
            36114945,  // Yuki-Onna, the Icicle Mayakashi
            37092104,  // Monstrous Arms of Atlantis
            37260677,  // Zoroa, the Magistus Verethragna
            37310367,  // Lockout Gardna
            37414347,  // F.A. Shining Star GT
            37442336,  // Cosmic Quasar Dragon
            37678339,  // Oneiros, the Dream Mirror Faeking
            37818794,  // Red-Eyes Dark Dragoon
            38491199,  // Junkuriboh
            38814750,  // PSY-Framegear Gamma
            38910263,  // Fire King Avatar Rangbali
            38916526,  // Rafale, Champion Fur Hire
            39138610,  // Allied Code Talker @Ignister
            39185163,  // Doomking Balerdroch
            39321065,  // Silent Swordsman Zero
            39468724,  // Nekroz of Areadbhair
            39915560,  // Starving Venom Predapower Fusion Dragon
            40139997,  // Ruddy Rose Dragon
            40939228,  // Shooting Majestic Star Dragon
            41782653,  // Overtex Qoatlus
            42110434,  // Naturia Butterfly
            43944080,  // Nemurelia's Dream Defender - Couette
            43989315,  // Invoked Sorath
            44201739,  // Mira Match
            44293356,  // Megalith Notrah Plura
            44508094,  // Stardust Dragon
            44508095,  // Stardust Dragon
            44508096,  // Stardust Dragon
            44665365,  // Herald of Perfection
            45002991,  // Crusadia Equimax
            45037489,  // Chevalier de Fleur
            45041488,  // Koa'ki Meiru Guardian
            45420955,  // Grozer the Thunderclap Tyrant
            45796834,  // Lightsky Maha Vailo
            45877457,  // Magikey Mechmortar - Garesglasser
            46033517,  // Machina Ruinforce
            46290741,  // Citadel Whale
            46396218,  // Azamina Ilia Silvia
            46613515,  // Clear Kuriboh
            46708514,  // Derk, Longing for the Blue Skies
            46935289,  // Herald of Mirage Lights
            47027714,  // T.G. Halberd Cannon/Assault Mode
            47759571,  // Pitknight Earlie
            47946130,  // Gouki The Giant Ogre
            48424886,  // El Shaddoll Grysta
            48546368,  // Herald of Ultimateness
            49161188,  // Gilti-Gearfried the Magical Steel Knight
            49275969,  // Nidhogg, Generaider Boss of Ice
            49456901,  // Number C104: Umbral Horror Masquerade
            49680980,  // Koa'ki Meiru Sandman
            49725936,  // Triple Burst Dragon
            49868263,  // Ryu Senshi
            50546029,  // G Golem Dignified Trilithon
            50954680,  // Crystal Wing Synchro Dragon
            51409648,  // Rindbrumm the Striking Dragon
            51447164,  // T.G. Blade Blaster
            51852507,  // Rose Archer
            52068432,  // Nekroz of Trishula
            52068433,  // Nekroz of Trishula
            52331012,  // Tri-Brigade Rugal the Silver Sheller
            52553102,  // Jurrac Astero
            53262004,  // Odd-Eyes Vortex Dragon
            53347303,  // Blue-Eyes Shining Dragon
            53618293,  // Sunvine Maiden
            53927851,  // Radiant Typhoon Varuroon, the Vibrant Vortex
            54178659,  // The Weather Painter Rainbow
            54603525,  // Clear Wing Synchro Dragon of the Four Heavenly Dragons
            54603526,  // Clear Wing Synchro Dragon of the Four Heavenly Dragons
            54842941,  // Enneacraft - Atori.MAR
            55063751,  // Gameciel, the Sea Turtle Kaiju
            55125728,  // Vera the Vernusylph Goddess
            55415564,  // D/D Evil
            55888045,  // Number C106: Giant Red Hand
            56532353,  // Neo Blue-Eyes Ultimate Dragon
            56863746,  // Drytron Meteonis DA Draconids
            57043986,  // Blue-Eyes Solid Dragon
            57450198,  // Super Quantal Mech Beast Lusterrex
            58058134,  // Slacker Magician
            58074177,  // Odd-Eyes Wing Dragon
            58083496,  // Odd-Eyes Override Dragon
            58468105,  // Dangerous Frightfur Nightmary
            59765225,  // Crystal Clear Wing Synchro Dragon
            60279710,  // Cyberse Enchanter
            60880471,  // Hellebore the Rikka Fairy
            60942444,  // Tenyi Spirit - Mapura
            61257789,  // Stardust Dragon/Assault Mode
            61257790,  // Stardust Dragon/Assault Mode
            61398234,  // Gate Guardian of Wind and Water
            61472381,  // Brotherhood of the Fire Fist - Eland
            61641818,  // Virtual World Dragon - Longlong
            61775475,  // Arms of Genex Return Zero
            61845881,  // Minairuka
            62587693,  // Deus X-Krawler
            63180841,  // Shooting Star Dragon T.G. - Expansion
            63265554,  // Power Tool Braver Dragon
            63767246,  // Number 38: Hope Harbinger Dragon Titanic Galaxy
            63941210,  // Jizukiru, the Star Destroying Kaiju
            64211118,  // Firewall Dragon Darkfluid - Neo Tempest Terahertz
            64454614,  // Arcana Extra Joker
            65172015,  // A-to-Z-Dragon Buster Cannon
            65240384,  // Big Shield Gardna
            65726770,  // Silent Magician Zero
            66102515,  // Kurikurinku @Ignister
            66192538,  // Danmari @Ignister
            66425726,  // Odd-Eyes Pendulumgraph Dragon
            66452432,  // Borrelshroud Dragon
            66722103,  // Thunderclap Monk
            66816282,  // Koa'ki Meiru Wall
            67098114,  // Loki, Lord of the Aesir
            67282505,  // Mitsu the Insect Ninja
            67688478,  // Gemini Imps
            67750322,  // Skull Meister
            68140974,  // Meklord Emperor Wisel
            68199168,  // Harr, Generaider Boss of Storms
            68679595,  // Ultimate Leo Utopia Ray
            68819554,  // Performage Damage Juggler
            68934651,  // Firewall Dragon Darkfluid
            68987122,  // Borrelguard Dragon
            69248256,  // Swordsoul Grandmaster - Chixiao
            69436288,  // Doom Kraken
            69680031,  // Dogmatika Fleurdelis, the Knighted
            69680032,  // Dogmatika Fleurdelis, the Knighted
            69946549,  // Predaplant Dragostapelia
            70333910,  // Noctovision Dragon
            70335319,  // All-Eyes Phantom Dragon
            70522875,  // Dragunity Lord - Georgius
            70636044,  // Varudras, the Final Bringer of the End Times
            71466592,  // Maryokutai
            71564252,  // Thunder King Rai-Oh
            71564253,  // Thunder King Rai-Oh
            71801447,  // Enneacraft - Atil.SPIA
            73129314,  // Heart of Gaia
            73452089,  // Dark Cavalry
            73642296,  // Ghost Belle & Haunted Mansion
            73642297,  // Ghost Belle & Haunted Mansion
            74203495,  // PSY-Framegear Delta
            74762582,  // Subterror Fiendess
            74822425,  // El Shaddoll Shekhinaga
            74892653,  // Stardust Warrior
            75286621,  // Invoked Mechaba
            75286622,  // Invoked Mechaba
            75402014,  // Ultimate Dragonic Utopia Ray
            75874514,  // Shining Star Dragon
            75892194,  // Ancient Gear Dragon
            75922381,  // Spright Red
            76078185,  // Heretical Phobos Covos
            76213610,  // Officiator of Doom Samuel
            76214441,  // Lifeforce Harmonizer
            76636978,  // Stardust Dragon - Victim Sanctuary
            76636979,  // Stardust Dragon - Victim Sanctuary
            77571454,  // Number 69: Heraldry Crest - Dark Matter Demolition
            78316184,  // Cyber Angel Vrash
            78661338,  // Fantastical Dragon Phantazmay
            78661339,  // Fantastical Dragon Phantazmay
            78700060,  // Doomcaliber Knight
            79559912,  // D/D/D Wave High King Caesar
            79606837,  // Herald of the Arc Light
            79621896,  // Archfiend's Origin
            79864860,  // Predaplant Triphyoverutum
            80086070,  // Ursarctic Grand Chariot
            80635735,  // Invoked Dee Anima
            80776622,  // DoSolfachord Coolia
            80870883,  // Red-Eyes Black Fullmetal Dragon
            80993256,  // Ashura King
            81020646,  // Void Ogre Dragon
            81418467,  // Primite Imperial Dragon
            82044279,  // Clear Wing Synchro Dragon
            82044280,  // Clear Wing Synchro Dragon
            82103466,  // Divine Serpent Geh
            82135803,  // Fiendsmith's Desirae
            82159583,  // Clown Crew Biancaviso
            82323997,  // Red Nova Dragon/Assault Mode
            82419869,  // U.A. Perfect Ace
            82699999,  // Live☆Twin Lil-la Sweet
            82744076,  // Tuning Gum
            82970905,  // Father Grizzly
            83257450,  // The Unstoppable Exodia Incarnate
            84330567,  // Tearlaments Rulkallos
            84343351,  // Crystal Clear Wing Over Synchro Dragon
            84521924,  // GranSolfachord Coolia
            84693918,  // Solfachord Solfegia
            84815190,  // Baronne de Fleur
            84899094,  // World Chalice Guardragon
            85523502,  // Whisker Blitzclique
            86937530,  // Fairy Tail - Luna
            87460579,  // Flower Cardian Lightflare
            88093706,  // Update Jammer
            88225269,  // Gem-Knight Hollowcore
            88234821,  // Dragunity Knight - Areadbhair
            88241506,  // Maiden with Eyes of Blue
            88890658,  // Baelgrill de Nouvelles
            88926295,  // Evigishki Neremanas
            89604813,  // Blue-Eyes Ultimate Spirit Dragon
            90036274,  // Clear Wing Fast Dragon
            90179822,  // Despian Comedy
            90555947,  // Naturia Gaiastrio
            90809975,  // Toadally Awesome
            91135480,  // Time Thief Doublebarrel
            92359409,  // Superdreadnought Rail Cannon Gustav Rocket
            92385016,  // Munin the Runick Wings
            92422871,  // Cyberse Disabwurm
            92565383,  // Cornfield Coatl
            93192592,  // Lollipo★Yummy Way
            94689635,  // Herald of Purple Light
            94798725,  // Gunkan Suship Uni-class Super-Dreadnought
            94919024,  // Lunalight Crimson Fox
            95095116,  // Gaia Prominence, the Fierce Force
            95243515,  // Virtual World Tiger - Huhu
            95372220,  // Flash Charge Dragon
            95626382,  // DoomZ XII End - Drastrius
            95824983,  // Electric Jellyfish
            96427353,  // Aqua Armor Ninja
            97268402,  // Effect Veiler
            97268403,  // Effect Veiler
            97268404,  // Effect Veiler
            97864322,  // Emperor Charles the Great
            98049934,  // Epurrely Beauty
            98127546,  // Saros Eresh Kurnugias
            98385955,  // Silvy of the White Forest
            98502113,  // Dark Paladin
            98502114,  // Dark Paladin
            98502115,  // Dark Paladin
            98502116,  // Dark Paladin
            98502117,  // Dark Paladin
            98630720,  // Borrelend Dragon
            99217226,  // Paladins of Bonds and Unity
            99902789,  // Chow Chow Chan
            100000049,  // Wisel Top 3 (TF6)
            100000309,  // Wisel Attack 5
            100430034,  // Baegriller de Nouvellez
            100458035,  // Sky Thunder Trirealm Rift Yomi
            100459006,  // Relinquished the Wicked Eye of Illusion
            100459009,  // Dark Paladin - Wave-Motion
            100459013,  // Evil Dragon Cards - Earth Dragon
            100460052,  // Ghostrick Master Alucard
            101104104,  // Dragonic Utopia Ray
            101111032,  // Evigishki Nereimanas
            101201043,  // Evolzar Razh
            101201048,  // Charlemagne
            101201103,  // Cosmic Quasar Dragon
            101402087,  // Skyborne Blue Cularsaw
            101403021,  // Heretic Eradicator
            101403028,  // Reindsurm Giriakarma
            101403032,  // Amazoness Swordmaster
            101403035,  // Omega Vidolium the Power Patron of Heaven's End
            101403044,  // Karin the Celestial Sage of the Highest Heights
            500000143,  // Centaur Mina
            504700019,  // A-Team: Trap Disposal Unit (GOAT)
            511000076,  // T.G. Blade Blaster (VG)
            511000183,  // Number 23: Lancelot, Dark Knight of the Underworld (Manga)
            511000260,  // Loki, Lord of the Aesir (Anime)
            511000294,  // Number C1000: Numeronius (Anime)
            511000670,  // Red-Eyes Darkness Metal Dragon (Anime)
            511000995,  // Timegazer Magician (Anime)
            511000997,  // Stargazer Magician (Anime)
            511001232,  // Singularity Fiend (Anime)
            511001275,  // Number 38: Hope Harbinger Dragon Titanic Galaxy (Manga)
            511001374,  // Number 8: Heraldic King Genom-Heritage (Anime)
            511001376,  // D/D/D Duo-Dawn King Kali Yuga (Anime)
            511001429,  // Number C102: Archfiend Seraph (Anime)
            511001776,  // Number 43: Manipulator of Souls (Anime)
            511001781,  // Number C6: Chronomaly Chaos Atlandis (Anime)
            511001965,  // Glacial Beast Iceberg Narwhal (Anime)
            511001997,  // Number 44: Sky Pegasus (Anime)
            511002034,  // Clear Wing Synchro Dragon (Anime)
            511002200,  // Void Ogre Dragon (Manga)
            511002227,  // Chevalier de Fleur (Anime)
            511002384,  // C/C/C Rock Armor of Battle
            511002477,  // Flower Cardian Lightflare (Anime)
            511002507,  // Suijin (Anime)
            511002508,  // Kazejin (Anime)
            511002681,  // Performage Magic Tactician
            511002871,  // Number C104: Umbral Horror Masquerade (Anime)
            511003047,  // Crystal Wing Synchro Dragon (Anime)
            511003201,  // Arcana Force EX - The Light Ruler (Anime)
            511003212,  // Rousing Hydradrive Monarch
            511003236,  // Unformed Void (Anime)
            511004412,  // Predaplant Stapeliaworm
            511007001,  // D/D Evil (Manga)
            511009049,  // Tyrant Red Dragon Archfiend (Anime)
            511009074,  // Dark Requiem Xyz Dragon (Anime)
            511009415,  // Starving Venemy Dragon
            511009665,  // Sunvine Maiden (Anime)
            511009685,  // Gouki Heel Ogre (Anime)
            511009717,  // Trident Hydradrive Lord
            511009935,  // Stormrider Tubieloof
            511009954,  // Linguriboh (Anime)
            511010009,  // Number 9: Dyson Sphere (Anime)
            511010025,  // Number 25: Force Focus (Anime)
            511010065,  // Number 65: Djinn Buster (Anime)
            511010104,  // Number 104: Masquerade (Anime)
            511010107,  // Number 107: Galaxy-Eyes Tachyon Dragon (Anime)
            511010239,  // Number C39: Utopia Ray Victory (Anime)
            511018001,  // Clear Kuriboh (Anime)
            511027044,  // Rose Archer (Manga)
            511027117,  // Gorgonic Guardian (Anime)
            511030028,  // Armatos Legio Magnus Dux
            511030061,  // Appliancer Dryer Drake (Anime)
            511369003,  // Stormrider Gulldiver
            511445002,  // Stormrider Harpiarm
            511600209,  // Nova Portal
            511600225,  // G Golem Dignified Trilithon (Anime)
            511600236,  // Drone Corporal
            511600238,  // Commandrone Double Sniper
            511600267,  // Marincess Wonder Heart (Anime)
            511600335,  // Salamangreat Zebroid X (Anime)
            511600406,  // Wisel Top 3 (TF5)
            513000012,  // Shooting Star Dragon (Anime)
            513000013,  // Red Nova Dragon (Anime)
            513000014,  // T.G. Halberd Cannon (Anime)
            513000015,  // T.G. Blade Blaster (Anime)
            513000038,  // Thunder King Rai-Oh (Manga)
            513000075,  // Malefic Stardust Dragon (Anime)
            513000078,  // Majestic Red Dragon (Anime)
            513000095,  // Sorcerer of Dark Magic (Anime)
            513000096,  // Clear Vice Dragon (Anime)
            513000140,  // Scab Scar Knight (Anime)
        };
    }
}
