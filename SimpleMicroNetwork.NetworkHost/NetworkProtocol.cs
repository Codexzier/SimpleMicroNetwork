using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleMicroNetwork.NetworkHost
{
    // TODO: Muss noch weiter ausgeführt werden.
    public class NetworkProtocol
    {
        private static StringBuilder sb = new StringBuilder();

        internal static string ReadReceiveMessage(string received)
        {
            // TODO: weitere information neben dem json daten objekt prüfen

            return received;
        }

        public static string CreateMessage(string jsonObject)
        {
            // TODO: weitere informationen für die Verbindung
            return jsonObject;
        }

        /// <summary>
        /// Erstellt aus den Empfangenden Daten und Auswertungen eine Antwort zusammen.
        /// </summary>
        /// <param name="isValid">Konnte der eingelesene Empfang gelesen werden?</param>
        /// <param name="passing">Wurde der Datensatz schonmal empfangen?</param>
        /// <param name="uid">UID des empfangenden Dateninhaltes.</param>
        /// <param name="receivedMessageLength">Länge des empfangenen Json Daten Objektes.</param>
        /// <param name="dt">The dateime is set of time creating with uid. It create only by host.</param>
        /// <returns>Gibt die Fertige Zusammstellung als Antwort Nachricht zurück.</returns>
        public static string CreateResponseMessage(bool isValid, short passing, long uid, int receivedMessageLength, DateTime dt)
        {
            sb.Clear();

            SetResponse(isValid ? NetworkProtocolResponse.Valid : NetworkProtocolResponse.Invalid);
            SetReceivePassing(passing);
            SetDateTime(dt);
            SetReceivedUid(uid);
            SetReceivedLength(receivedMessageLength);

            return sb.ToString();
        }

        internal static string CreateResponseMessageError(short passing, long uid, int receivedMessageLength, DateTime dt)
        {
            sb.Clear();

            SetResponse(NetworkProtocolResponse.Error);
            SetReceivePassing(passing);
            SetDateTime(dt);
            if (passing > 0)
            {
                SetReceivedUid(uid);
            }
            SetReceivedLength(receivedMessageLength);

            return sb.ToString();
        }

        private static void SetResponse(NetworkProtocolResponse respsoneText)
        {
            string text = string.Empty;
            switch (respsoneText)
            {
                case NetworkProtocolResponse.Valid:
                    text = "VALID";
                    break;
                case NetworkProtocolResponse.Invalid:
                    text = "INVALID";
                    break;
                case NetworkProtocolResponse.Error:
                    text = "ERROR";
                    break;
                default:
                    throw new InvalidOperationException("response message option not exist");
            }

            sb.AppendLine($"RESPONSE: {text}");
        }

        private static void SetReceivePassing(short passing)
        {
            if (passing > 0)
            {
                sb.AppendLine($"RECEIVED PASSING: {passing}");
            }
        }

        private static void SetDateTime(DateTime dt)
        {
            sb.AppendLine($"RECEIVED DATETIME: {dt.Year}-{dt.Month}-{dt.Day}T{dt.Hour}:{dt.Minute}:{dt.Second}");
        }

        private static void SetReceivedUid(long uid)
        {
            sb.AppendLine($"RECEIVED UID: {uid}");
        }

        private static void SetReceivedLength(int receivedMessageLength)
        {
            sb.AppendLine($"RECEIVED LENGTH: {receivedMessageLength}");
        }

    }
}
