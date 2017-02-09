﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LCARS.Models;
using LCARS.ViewModels.Builds;

namespace LCARS.Services
{
    public class BuildsService : IBuildsService
    {
        private readonly Settings _settings;

        public BuildsService(ISettingsService settingsService)
        {
            _settings = settingsService.GetSettings();
        }

        public async Task<Dictionary<string, int>> GetBuildsRunning()
        {
            var doc = await GetData($"http://{_settings.BuildServerUrl}/guestAuth/app/rest/builds?locator=running:true");

            var builds = new Dictionary<string, int>();

            if (doc == null)
            {
                return builds;
            }

            foreach (
                var build in
                doc.Root.Elements("build").Where(build => !builds.ContainsKey(build.Attribute("buildTypeId").Value))
            )
            {
                builds.Add(build.Attribute("buildTypeId").Value, Convert.ToInt32(build.Attribute("id").Value));
            }

            return builds;
        }

        public async Task<BuildProgress> GetBuildProgress(int buildId)
        {
            var doc =
                await GetData($"http://user:pwd@{_settings.BuildServerUrl}/guestAuth/app/rest/builds/id:{buildId}");

            if (doc == null)
            {
                return new BuildProgress
                {
                    Percentage = 0,
                    Status = ""
                };
            }

            var runningInfo = doc.Root.Element("running-info");

            if (runningInfo == null)
            {
                return new BuildProgress
                {
                    Status = doc.Root.Element("statusText").Value,
                    Percentage = 100
                };
            }

            return new BuildProgress
            {
                Status = runningInfo.Attribute("currentStageText").Value,
                Percentage = Convert.ToInt32(runningInfo.Attribute("percentageComplete").Value)
            };
        }

        public async Task<KeyValuePair<string, string>> GetLastBuildStatus(string buildTypeId)
        {
            var doc =
                await
                    GetData(
                        $"http://{_settings.BuildServerUrl}/guestAuth/app/rest/builds?locator=buildType:(id:{buildTypeId})");

            if (doc == null)
            {
                return new KeyValuePair<string, string>("Unknown", "Unknown");
            }

            return new KeyValuePair<string, string>(doc.Root.Element("build").Attribute("status").Value,
                doc.Root.Element("build").Attribute("number").Value);
        }

        private async Task<XDocument> GetData(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes($"{_settings.BuildServerUsername}:{_settings.BuildServerPassword}")));

                var xml = await client.GetStreamAsync(url);

                var doc = XDocument.Load(xml);

                return doc.Root == null ? null : doc;
            }
        }
    }
}