using SimpleMicroNetwork.NetworkData;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleMicroNetwork.NetworkHost
{
    /// <summary>
    /// Mit dem Backlog soll ermöglicht werden, 
    /// zuviele Empfangene Daten kurz Zeitig zu halten 
    /// und für die spätere weiter verarbeitung rauszugeben.
    /// </summary>
    public class NetworkBacklog
    {
        private Queue<NetworkBacklogItem> _backlogItems;
        private int _maxBacklog;

        /// <summary>
        /// Ruft die aktuelle Menge der Empfangenen Backlogs
        /// </summary>
        public int Count => this._backlogItems.Count;

        /// <summary>
        /// Ruft die eingestellte maximale Backlog größe ab.
        /// </summary>
        public int MaxBacklog { get { return this._maxBacklog; } }

        /// <summary>
        /// Instanziert das Backlog, mit dem der eingehende Empfang gelagert und abgerufen werden kann 
        /// </summary>
        /// <param name="maxBacklog">Legt die Backlog Größe fest.</param>
        public NetworkBacklog(int maxBacklog)
        {
            this._maxBacklog = maxBacklog;
            this._backlogItems = new Queue<NetworkBacklogItem>(maxBacklog);
        }

        /// <summary>
        /// Fügt ein Backlog ergebnis hinzu. Ist der Backlog voll, dann wird ein fehler zurück gemeldet per event.
        /// </summary>
        /// <param name="item"></param>
        public void Add(NetworkBacklogItem item)
        {
            if (this.CheckMaxBacklog())
            {
                return;
            }

            this._backlogItems.Enqueue(item);
        }

        /// <summary>
        /// Prüft die menge des Backlogs und meldet per Event, wenn dieser voll ist.
        /// </summary>
        /// <returns>Gibt true zurück, wenn das Backlog unter dem Maximum liegt.</returns>
        private bool CheckMaxBacklog()
        {
            bool result = this._backlogItems.Count >= this._maxBacklog;
            if (result)
            {
                this.BacklogFullEvent?.Invoke(this, new NetworkMessageEventArgs("Backlog is full!"));
            }

            return result;
        }

        public delegate void BacklogFullEventHandler(object sender, NetworkMessageEventArgs args);
        public event BacklogFullEventHandler BacklogFullEvent;
    }
}
