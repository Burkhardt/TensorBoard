using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using OsLib;
using System.Linq;
using System.Threading.Tasks;
using RunProcessAsTask; // https://github.com/jamesmanning/RunProcessAsTask

namespace TB
{
    public class TensorboardInfo
    {
        public TensorboardInfo(dynamic obj)
        : this((JObject)JObject.FromObject(obj))
        {
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="jo">port, logDir</param>
        public TensorboardInfo(JObject jo)
        {
            Port = (int)jo["port"];
            LogDir = new RaiFile((string)jo["logDir"]).Path;
            Name = new RaiFile(LogDir.Length < 2 ? "" : LogDir.Substring(0, LogDir.Length - 1)).Name;
            Url = $"http://localhost:{Port}/";
        }
        public TensorboardInfo(int port = 6006, string logDir = "~/HelloClass.Logs/")
        {
            Port = port;
            LogDir = logDir;
            Name = new RaiFile(LogDir.Length < 2 ? "" : LogDir.Substring(0, LogDir.Length - 1)).Name;
            Url = $"http://localhost:{Port}/";
        }
        public string LogDir
        {
            get
            {
                return logDir;
            }
            set
            {
                logDir = new RaiFile(value).Path;
                Name = new RaiFile(logDir.Length < 2 ? "" : logDir.Substring(0, LogDir.Length - 1)).Name;
            }
        }
        private string logDir = null;
        public string Name { get; set; }
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = Math.Max(6006, Math.Min(value, 6008));
                Url = $"http://localhost:{Port}/";
            }
        }
        private int port;
        public bool Running { get; set; }
        public int Pid { get; set; }
        public string Url { get; set; }
    }
    /// <summary>
    /// For Tensorboards, we use the following convention:
    /// next to each .pmml file is a directory by the same name with the extension .log
    /// which will be used by the tensorboard server instance to scan training results.
    /// The name of the Tensorboard instance is the Directory name without extension.
    /// </summary>
    /// <typeparam name="string">key</typeparam>
    /// <typeparam name="NotebookInfo">info</typeparam>
    public class Tensorboard : IEnumerable<KeyValuePair<string, TensorboardInfo>>
    {
        private static ConcurrentDictionary<string, TensorboardInfo> tensorboards =
            new ConcurrentDictionary<string, TensorboardInfo>();
        public TensorboardInfo this[string key]
        {
            get
            {
                return tensorboards.ContainsKey(key) ? tensorboards[key] : null;
            }
            set
            {
                tensorboards[key] = value;
            }
        }
        public IEnumerator<KeyValuePair<string, TensorboardInfo>> GetEnumerator() => tensorboards.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => tensorboards.GetEnumerator();
        protected static string TensorboardCmd { get; set; } = "tensorboard";
        protected static string TensorboardParam { get; set; } = "--logdir \"{LogDir}\" --port {Port}";
        protected static string LsofCmd { get; set; } = "lsof -i :{Port}";
        protected static string PsCmd { get; set; } = "ps -o pid= -o command= -p {Pid}";
        protected static string KillCmd { get; set; } = "kill {Pid}";
        public static List<int> PortRange = Enumerable.Range(6006, 3).ToList();
        public string Name { get; set; }
        public int Port => tensorboards[Name].Port;
        public TensorboardInfo Info
        {
            get { return tensorboards[Name]; }
            set { tensorboards[Name] = value; }
        }
        /// <summary>
        /// return a link to a running notebook
        /// </summary>
        /// <param name="server"></param>
        /// <returns>link or null</returns>
        public string Link(string server)
        {
            if (!tensorboards.ContainsKey(Name))
                return null;
            return $"{Info.Url}"
                    .Replace("http://", "https://")
                    .Replace("localhost", server)
                    .Replace(":6006", "/tb/1")
                    .Replace(":6007", "/tb/2")
                    .Replace(":6008", "/tb/3");
            //.Replace(".log", "");
        }
        public static TensorboardInfo Status(int port)
        {
            #region example lsof output
            // COMMAND     PID USER   FD   TYPE             DEVICE SIZE/OFF NODE NAME
            // python3.6 96254  RSB    6u  IPv4 0x89dc4c14217060f9      0t0  TCP *:6006 (LISTEN)
            // ----one special output
            // McNabb:RaiUtils20180929 RSB$ lsof -i :6006
            // COMMAND     PID USER   FD   TYPE             DEVICE SIZE/OFF NODE NAME
            // com.apple  7156  RSB    5u  IPv4 0xdc021b9835418dbb      0t0  TCP localhost:54316->localhost:6006 (ESTABLISHED)
            // com.apple  7156  RSB    8u  IPv4 0xdc021b9831cd7dbb      0t0  TCP localhost:54317->localhost:6006 (ESTABLISHED)
            // com.apple  7156  RSB   10u  IPv4 0xdc021b9836a3643b      0t0  TCP localhost:54318->localhost:6006 (ESTABLISHED)
            // python3.6 76551  RSB    6u  IPv4 0xdc021b983711ddbb      0t0  TCP *:6006 (LISTEN)
            // python3.6 76551  RSB    9u  IPv4 0xdc021b984983ddbb      0t0  TCP localhost:6006->localhost:54316 (ESTABLISHED)
            // python3.6 76551  RSB   10u  IPv4 0xdc021b98358a8abb      0t0  TCP localhost:6006->localhost:54317 (ESTABLISHED)
            // python3.6 76551  RSB   17u  IPv4 0xdc021b9841c59dbb      0t0  TCP localhost:6006->localhost:54318 (ESTABLISHED)
            // ----end
            // lsof returns 1 if the port is not used and outputs nothing
            #endregion
            #region example ps output
            // McNabb:RaiUtils20180918 RSB$ ps -o pid= -o command= -p 3019
            // 3019 /Users/RSB/anaconda/bin/python /Users/RSB/anaconda/bin/tensorboard --logdir=/Users/RSB/Tensorboard.LogDir
            // 8120 /Users/RSB/anaconda/bin/python /Users/RSB/anaconda/bin/tensorboard --logdir /Users/RSB/models/KerasMobileNet/ --port 6007
            // 43664 /Users/RSB/anaconda/bin/python /Users/RSB/anaconda/bin/tensorboard --logdir /Volumes/DroboFront/DropboxRainer/Dropbox (HDitem)/ZementisModeler/ --port 6008
            // McNabb:RaiUtils20180929 RSB$ lsof -i :6008
            // McNabb:RaiUtils20180929 RSB$ lsof -i :6008
            #endregion
            var info = new TensorboardInfo();
            string lsofOut = "";
            var lsof = new RaiSystem(LsofCmd.Replace("{Port}", (port).ToString()));
            int rc = lsof.Exec(out lsofOut);
            if (rc == 0)
            {
                var q = from _ in lsofOut.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        where _.Contains("(LISTEN)")
                        select _;
                var list = q.ToList();
                if (list.Count() > 0)
                {
                    #region get Pid from lsof
                    var lsofValues = list.First().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lsofValues.Count() > 1)
                    {
                        info.Pid = int.Parse(lsofValues[1]); // 96254 in the example above
                        info.Running = true;
                    }
                    #endregion
                    #region get Name and LogDir from ps
                    var ps = new RaiSystem(PsCmd.Replace("{Pid}", info.Pid.ToString()));
                    string psOut = "";
                    rc = ps.Exec(out psOut);
                    if (rc == 0)
                    {
                        int l = psOut.IndexOf("--logdir") + 9, r = psOut.IndexOf("--port");
                        info.LogDir = new RaiFile(psOut.Substring(l, r - l)).Path;
                        info.Name = new RaiFile(info.LogDir.Substring(0, info.LogDir.Length - 1)).Name;
                        var psValues = psOut.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        info.Pid = int.Parse(psValues[0]);
                        info.Port = int.Parse(psOut.Substring(r + 7));
                        info.Url = $"http://localhost:{info.Port}";
                        return info;
                    }
                    #endregion 
                }
            }
            return null;
        }
        /// <summary>
        /// Checks if tensorboard is currently running on any of the three ports
        /// </summary>
        public static void Refresh()
        {
            #region check ports in PortRange
            foreach (var portNumber in PortRange)
            {
                var info = Status(portNumber);  // get current info from OS
                if (info != null)
                {
                    #region update tensorboards 
                    var infoName = string.IsNullOrEmpty(info.Name) ? $"default{portNumber}" : info.Name;
                    tensorboards[infoName] = info;
                    #endregion
                }
                else
                {
                    #region remove item with this port - nothing running there
                    var q = from _ in tensorboards
                            where _.Value.Port == portNumber
                            select _.Value;
                    if (q.Count() > 0)
                    {
                        var removeInfo = q.First();
                        tensorboards.TryRemove(removeInfo.Name, out info);
                    }
                    #endregion
                }
            }
            #endregion
        }
        /// <summary>
        /// Start Tensorboard
        /// may take a while before this process is all ramped up and responsive
        /// </summary>
        /// <param name="refresh"></param>
        public async Task Start(bool refresh = true)
        {
            if (tensorboards.ContainsKey(Name))
            {
                if (Name != Info.Name)    // is this possible?
                    throw new Exception("internal error");
                if (Info.Running)
                    return; // nothing todo since this tensorboard is already running
                //else Info.Port = tensorboards[Name].Port;
            }
            var sys = new RaiSystem(
                TensorboardCmd,
                TensorboardParam
                .Replace("{LogDir}", Info.LogDir)
                .Replace("{Port}", Port.ToString()));  // use the port as assigned in constructor
            tensorboards[Name].Running = true;
            //tensorboards[Name] = Info;  // add or replace
            await sys.Start();
        }
        /// <summary>
        /// Stop Tensorboard
        /// </summary>
        /// <param name="refresh">true, refresh state from OS</param>
        /// <returns>true if Tensorboard is not running (anymore)</returns>
        public void Stop()
        {
            #region not running
            TensorboardInfo info = null;
            string sOut = "";
            int rc;
            if (!IsRunning())
                return;
            if (Info.Pid == 0)
                throw new InvalidOperationException("precondition violated: Pid has invalid value");
            #endregion
            #region kill it as long as it is still running
            for (int i = 0; i < 2 && IsRunning(); i++)
            {
                var sys = new RaiSystem(KillCmd.Replace("{Pid}", Info.Pid.ToString()));
                info = Info;
                rc = sys.Exec(out sOut);
                if (rc != 0 && !sOut.Contains("No such process"))
                    Task.Delay(1000).Wait();
            }
            #endregion
            #region update dictionary 
            // for (int i = 0; i < 10 && !tensorboards.TryRemove(Name, out info); i++)
            //     Task.Delay(100).Wait();
            tensorboards.TryRemove(Name, out info);
            #endregion
        }
        public static void StopAll()
        {
            for (var tbList = Tensorboard.List(true); tbList.Count() > 0; tbList = Tensorboard.List(true))
            {
                tbList.First().Stop();
                Task.Delay(1000).Wait();
            }
        }
        public bool IsRunning(bool refresh = true)
        {
            if (refresh)
                Refresh();
            return tensorboards.ContainsKey(Name) ? Info.Running : false;
        }
        public static bool NextPortAvailable(out int port)
        {
            port = 0;
            foreach (var portNumber in PortRange)
            {
                if (Status(portNumber) == null)
                {
                    port = portNumber;
                    return true;
                }
            }
            return false;
        }
        public static Tensorboard Find(string search, bool refresh = true)
        {
            var name = new RaiFile(search).Name;
            if (refresh)
                Refresh();
            var q = from _ in tensorboards
                    where _.Key.Contains(name) || _.Value.Name.Contains(name)
                    select _.Value;
            return q.Count() > 0 ? new Tensorboard((TensorboardInfo)q.First()) : null;
        }
        public static List<Tensorboard> List(bool refresh = true)
        {
            if (refresh)
                Refresh();
            var q = from _ in tensorboards select new Tensorboard(_.Value);
            return q.ToList();
        }
        public Tensorboard(TensorboardInfo info)
        {
            // http://localhost:8890/?token=0da788a158d220e91c783e1014b0f7ba21c829ba07974692
            Name = info.Name;
            Info = info;
        }
        public Tensorboard(JObject jo)
        {
            var info = new TensorboardInfo(jo);
            Name = info.Name;
            Info = info;
        }
    }
}