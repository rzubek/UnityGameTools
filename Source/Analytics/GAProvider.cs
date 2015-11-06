using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SomaSim.Analytics
{
    /// <summary>
    /// Simple implementation of the Google Analytics REST API.
    ///
    /// https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide
    /// </summary>
    public class GAProvider : IAnalyticsProvider
    {
        // configured in the constructor
        private string _uaid;
        private string _cid;

        private GAProviderQueryBuilder _builder;
        private System.Random _rng;

        private string _preamble; // this part of the query won't change during a single session

        private readonly string URL = "http://www.google-analytics.com/collect?";

        public GAProvider (string analyticsID, string clientID) {
            _uaid = analyticsID;
            _cid = clientID;
        }

        public void Initialize () {
            _builder = new GAProviderQueryBuilder();
            _rng = new System.Random();

            // put together a standard preamble
            _builder.ClearParams();
            _builder.AddPair("v", "1");
            _builder.AddPair("tid", _uaid);
            _builder.AddPair("cid", _cid);
            _preamble = _builder.ToQueryString(URL);

            LogSession(true);
        }

        public void Release () {
            LogSession(false);
        }

        public void Update () {
            // no op in this implementation
        }

        //
        // public api

        public void LogEvent (params string[] arguments) {
            LogPageview(MakeEncodedPath(arguments));
        }

        //
        // implementation details

        private void LogSession (bool start) {
            _builder.ClearParams();
            _builder.AddPair("sc", start ? "start" : "end");

            SendQuery();
        }

        private void LogPageview (string encodedPath) {
            _builder.ClearParams();
            _builder.AddPair("t", "pageview");
            _builder.AddPair("dp", encodedPath);

            SendQuery();
        }

        private void SendQuery () {
            _builder.AddPair("z", _rng.Next().ToString());
            string query = _builder.ToQueryString(_preamble);

#pragma warning disable 0219 // www is assigned but its value is never used
            WWW www = new WWW(query);
            // aaaand drop any errors on the floor
#pragma warning restore 0219

        }

        private StringBuilder _pathsb = new StringBuilder(); // gc helper
        private string MakeEncodedPath (string[] arguments) {
            _pathsb.Length = 0;
            if (arguments != null) {
                foreach (string arg in arguments) {
                    _pathsb.Append("/");
                    _pathsb.Append(WWW.EscapeURL(arg));
                }
            }

            string result = (_pathsb.Length == 0) ? "/" : _pathsb.ToString();
            _pathsb.Length = 0;
            return result;
        }
    }

    //
    // implementation details

    internal class GAProviderQueryBuilder
    {
        private struct KeyValuePair
        {
            public string Key;
            public string Value;
        }

        private StringBuilder _sb = new StringBuilder();
        private List<KeyValuePair> _params = new List<KeyValuePair>();

        public void ClearParams () {
            _params.Clear();
        }

        public void AddPair (string escapedKey, string escapedValue) {
            _params.Add(new KeyValuePair() { Key = escapedKey, Value = escapedValue });
        }

        public string ToQueryString (string preamble) {
            if (preamble != null) {
                _sb.Append(preamble);
            }

            for (int i = 0, count = _params.Count; i < count; i++) {
                var pair = _params[i];

                if (i > 0) { _sb.Append("&"); }
                _sb.Append(pair.Key);
                _sb.Append("=");
                _sb.Append(pair.Value);
            }

            string result = _sb.ToString();
            _sb.Length = 0;

            return result;
        }
    }
}
