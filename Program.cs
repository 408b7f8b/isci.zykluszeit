using System;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using isci.Allgemein;
using isci.Daten;
using isci.Beschreibung;

namespace isci.zykluszeit
{
    class Program
    {
        public class Konfiguration : Parameter
        {
            [fromArgs, fromEnv]
            public uint anlaufZyklen = 100;
            public Konfiguration(string[] args) : base(args) {}
        }

        static void Main(string[] args)
        {
            var konfiguration = new Konfiguration(args);
            
            var structure = new Datenstruktur(konfiguration);
            var ausfuehrungsmodell = new Ausführungsmodell(konfiguration, structure.Zustand);

            var dm = new Datenmodell(konfiguration.Identifikation);

            var zykluszeit = new dtDouble(0.0, "zykluszeit");
            var zykluszeit_max = new dtDouble(0.0, "zykluszeit_max");
            dm.Dateneinträge.Add(zykluszeit);
            dm.Dateneinträge.Add(zykluszeit_max);

            var beschreibung = new Modul(konfiguration.Identifikation, "isci.zykluszeit", new ListeDateneintraege(){zykluszeit, zykluszeit_max});
            beschreibung.Name = "Zykluszeit Ressource " + konfiguration.Identifikation;
            beschreibung.Beschreibung = "Modul zur Zykluszeitermittlung";
            beschreibung.Speichern(konfiguration.OrdnerBeschreibungen + "/" + konfiguration.Identifikation + ".json");

            dm.Speichern(konfiguration.OrdnerDatenmodelle + "/" + konfiguration.Identifikation + ".json");

            structure.DatenmodellEinhängen(dm);
            structure.Start();

            zykluszeit_max.Wert = 0.0;
            zykluszeit_max.WertInSpeicherSchreiben();

            long curr_ticks = 0;

            int i = 0;
            
            while(true)
            {
                structure.Zustand.WertAusSpeicherLesen();

                if (ausfuehrungsmodell.AktuellerZustandModulAktivieren())
                {
                    var curr_ticks_new = System.DateTime.Now.Ticks;
                    var ticks_span = curr_ticks_new - curr_ticks;
                    curr_ticks = curr_ticks_new;
                    zykluszeit.Wert = (double)ticks_span / System.TimeSpan.TicksPerMillisecond;
                    zykluszeit.WertInSpeicherSchreiben();

                    if (zykluszeit.Wert > zykluszeit_max.Wert && i > konfiguration.anlaufZyklen)
                    {
                        zykluszeit_max.Wert = zykluszeit.Wert;
                        zykluszeit_max.WertInSpeicherSchreiben();
                    }

                    if (i <= konfiguration.anlaufZyklen) ++i;

                    ausfuehrungsmodell.Folgezustand();
                    structure.Zustand.WertInSpeicherSchreiben();
                }

                System.Threading.Thread.Sleep(1);
            }
        }
    }
}