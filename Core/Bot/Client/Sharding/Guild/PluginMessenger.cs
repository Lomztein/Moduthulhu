using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class PluginMessenger
    {
        private List<MessageInfo> _messageRegister = new List<MessageInfo>();

        public object SendMessage (string target, string name, object value = null)
        {
            List<MessageInfo> infos = _messageRegister.Where(x => x.Matches(target, name)).ToList ();
            MessageInfo single = infos.SingleOrDefault();
            if (single != null)
            {
                return single.Execute(value);
            }
            else Log.Write(Log.Type.WARNING, $"No actions/functions registered in PluginMessenger that matched target = '{target}' and name = '{name}'. Messsage not sent.");
            return null;
        }

        public T SendMessage<T>(string target, string name, object value = null) => (T)SendMessage(target, name, value);

        public void Register(string target, string name, Func<object, object> function) => Register(target, name, new MessageInfo (target, name, function));
        public void Register(string target, string name, Action<object> action) => Register (target, name, new MessageInfo (target, name, action));
        private void Register (string target, string name, MessageInfo info)
        {
            if (_messageRegister.Any(x => x.Matches(target, name)))
            {
                Log.Write(Log.Type.CRITICAL, $"Trying to register action/function with target = '{target}' and name = '{name}', but an identical one already exists. Registration cancelled.");
                return;
            }
            Log.Write(Log.Type.PLUGIN, $"Succesfully registered action/function with target = '{target}' and name = '{name}'!");
            _messageRegister.Add(info);
        }
        public void Unregister(string target, string name)
        {
            MessageInfo info = _messageRegister.FirstOrDefault(x => x.Matches(target, name));
            if (info == null)
            {
                Log.Write(Log.Type.WARNING, $"Attempted to remove action/function with target = '{target}' and name = {name}, but none such exists.");
            }else _messageRegister.Remove(info);
        }

        public void Clear (string target)
        {
            Log.Write(Log.Type.PLUGIN, $"Clearing all registered message actions/functions for target = '{target}'.");
            _messageRegister.RemoveAll(x => x.Matches(target));
        }

        private class MessageInfo
        {
            private string _target;
            private string _name;

            private Func<object, object> _function;

            public MessageInfo (string target, string name, Func<object, object> function)
            {
                _target = target;
                _name = name;
                _function = function;
            }

            public MessageInfo (string target, string name, Action<object> action)
            {
                _target = target;
                _name = name;
                _function = (x) => { action(x); return null; };
            }

            public bool Matches(string target) => _target == target;
            public bool Matches (string target, string name) => Matches(target) && _name == name;

            public object Execute(object value) => _function(value);
        }
    }
}
