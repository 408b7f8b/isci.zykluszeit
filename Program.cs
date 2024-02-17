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
        static void Main(string[] args)
        {
            var konfiguration = new Parameter(args);

            var structure = new Datenstruktur(konfiguration);

            var dm = new Datenmodell(konfiguration);
            var zykluszeit = new dtInt32(0, "LetzteZykluszeit");
            dm.Dateneinträge.Add(zykluszeit);

            var beschreibung = new Modul(konfiguration.Identifikation, "isci.zykluszeit", new ListeDateneintraege(){zykluszeit});
            beschreibung.Name = "Zykluszeit Ressource " + konfiguration.Identifikation;
            beschreibung.Beschreibung = "Modul zur Zykluszeitermittlung";
            beschreibung.Speichern(konfiguration.OrdnerBeschreibungen + "/" + konfiguration.Identifikation + ".json");

            dm.Speichern(konfiguration.OrdnerDatenmodelle + "/" + konfiguration.Identifikation + ".json");

            structure.DatenmodellEinhängen(dm);
            structure.Start();

            var ausfuehrungsmodell = new Ausführungsmodell(konfiguration, structure.Zustand);

            long curr_ticks = 0;
            
            while(true)
            {
                structure.Zustand.WertAusSpeicherLesen();

                if (ausfuehrungsmodell.AktuellerZustandModulAktivieren()){
                    var curr_ticks_new = System.DateTime.Now.Ticks;
                    var ticks_span = curr_ticks_new - curr_ticks;
                    curr_ticks = curr_ticks_new;
                    zykluszeit.Wert = (System.Int32)(ticks_span / System.TimeSpan.TicksPerMillisecond);
                    zykluszeit.WertInSpeicherSchreiben();

                    ausfuehrungsmodell.Folgezustand();
                    structure.Zustand.WertInSpeicherSchreiben();
                }
            }
        }
    }
}