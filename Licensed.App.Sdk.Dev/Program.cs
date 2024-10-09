using Licensed.App.Sdk;
using Newtonsoft.Json;
using System.Diagnostics;

const string appId = "7ee0f0e8-e92f-4578-94eb-af64cee1067c";
const string licenseKey = "7ULE-2HT8-YGQJ-3WUY";

using var application = new LicensedApp(appId);

var res = await application.Connect(licenseKey);

Console.WriteLine(JsonConvert.SerializeObject(res));