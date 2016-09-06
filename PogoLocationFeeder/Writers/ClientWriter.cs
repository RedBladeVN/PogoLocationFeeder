/*
PogoLocationFeeder gathers pokemon data from various sources and serves it to connected clients
Copyright (C) 2016  PogoLocationFeeder Development Team <admin@pokefeeder.live>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Caching;
using Newtonsoft.Json;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Writers
{
    public class ClientWriter
    {
        public static readonly ClientWriter Instance = new ClientWriter();

        private MemoryCache cache = new MemoryCache("PokemonMemoryCache");
        private HttpListener httpListener = new HttpListener();

        private ClientWriter()
        {
        }

        public async void StartNet(int port)
        {
            try
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(string.Format("http://localhost:{0}/", port));
                httpListener.Start();

                await Task.Factory.StartNew(() => StartAccept());
            }
            catch (SocketException e)
            {
                Log.Fatal($"Port {port} is already in use!", e);
                throw e;
            }
        }

        private async void StartAccept()
        {
            while (!GlobalSettings.ThreadPause)
            {
                HttpListenerContext context = await httpListener.GetContextAsync();

                Log.Info($"New request from {context.Request.UserHostAddress}");

                await Task.Factory.StartNew(() => HandleAccept(context));
            }
        }

        private void HandleAccept(HttpListenerContext context)
        {
            lock (cache)
            {
                PogoBotSniperInfoContainer container = new PogoBotSniperInfoContainer();

                // Build up the pokemon list (cant directly retrieve the MemoryCache objects)
                foreach (KeyValuePair<string, object> pair in cache)
                {
                    PogoBotSniperInfo pogoInfo = pair.Value as PogoBotSniperInfo;

                    // It might have been really expired but MemoryCache havent removed it
                    if (!pogoInfo.IsExpired)
                    {
                        container.Infos.Add(pair.Value as PogoBotSniperInfo);
                    }
                }

                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(container, Formatting.Indented));

                context.Response.OutputStream.Write(data, 0, data.Length);
                context.Response.OutputStream.Close();
            }
        }

        public void Update(List<SniperInfo> infos)
        {
            lock (cache)
            {
                // Validate and convert informations
                foreach (var info in infos)
                {
                    cache.Add(info.ToString(), new PogoBotSniperInfo(info), new DateTimeOffset(info.ExpirationTimestamp));
                }
            }
        }

        public bool IsWorking()
        {
            return httpListener.IsListening;
        }
    }
}
