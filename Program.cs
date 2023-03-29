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
            var konfiguration = new Parameter("konfiguration.json");
            
            var structure = new Datenstruktur(konfiguration.OrdnerDatenstruktur);

            var dm = new Datenmodell(konfiguration.Identifikation);
            var zykluszeit = new dtInt32(0, "zykluszeit");
            dm.Dateneinträge.Add(zykluszeit);

            var beschreibung = new Modul(konfiguration.Identifikation, "isci.zykluszeit", new ListeDateneintraege(){zykluszeit});
            beschreibung.Name = "Zykluszeit Ressource " + konfiguration.Identifikation;
            beschreibung.Beschreibung = "Modul zur Zykluszeitermittlung";
            beschreibung.Speichern(konfiguration.OrdnerBeschreibungen + "/" + konfiguration.Identifikation + ".json");

            dm.Speichern(konfiguration.OrdnerDatenmodelle + "/" + konfiguration.Identifikation + ".json");

            structure.DatenmodellEinhängen(dm);
            structure.Start();

            var Zustand = new dtInt32(0, "Zustand", konfiguration.OrdnerDatenstruktur + "/Zustand");
            Zustand.Start();

            long curr_ticks = 0;

            bool dbg = System.Environment.UserInteractive;
            
            while(true)
            {
                System.Threading.Thread.Sleep(10);
                Zustand.Lesen();

                if (dbg)
                {
                    System.Console.Clear();
                    System.Console.WriteLine("Zustand: " + (System.Int32)Zustand.value);
                    System.Console.WriteLine("Zykluszeit: " + (System.Int32)zykluszeit.value);
                }

                var erfüllteTransitionen = konfiguration.Ausführungstransitionen.Where(a => a.Eingangszustand == (System.Int32)Zustand.value);
                if (erfüllteTransitionen.Count<Ausführungstransition>() > 0)
                {
                    var curr_ticks_new = System.DateTime.Now.Ticks;
                    var ticks_span = curr_ticks_new - curr_ticks;
                    curr_ticks = curr_ticks_new;
                    zykluszeit.value = (System.Int32)(ticks_span / System.TimeSpan.TicksPerMillisecond);
                    zykluszeit.Schreiben();

                    Zustand.value = erfüllteTransitionen.First<Ausführungstransition>().Ausgangszustand;
                    Zustand.Schreiben();
                }
            }
        }
    }
}