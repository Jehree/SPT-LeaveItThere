using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeaveItThere.Common
{
    public class Interaction
    {
        public Action Action { get; private set; }
        public Func<string> GetName { get; private set; }
        public Func<bool> GetEnabled { get; private set; }
        public Func<string> GetTargetName { get; private set; }
        public bool RefreshEnabled { get; private set; }
        private static Action _refreshPrompt = new(InteractionHelper.RefreshPrompt);

        public class Builder
        {
            private Func<string> _getName = () => "Default Interaction Name";
            private Func<bool> _getEnabled = () => true;
            private Action _action = () => { };
            private Func<string> _targetName = () => null;
            private bool _refreshEnabled = false;

            public Builder SetName(string name)
            {
                _getName = () => name;
                return this;
            }

            public Builder SetName(Func<string> getName)
            {
                _getName = getName;
                return this;
            }

            public Builder SetTargetName(string targetName)
            {
                _targetName = () => targetName;
                return this;
            }

            public Builder SetTargetName(Func<string> getTargetName)
            {
                _targetName = getTargetName;
                return this;
            }

            public Builder SetEnabled(bool enabled)
            {
                _getEnabled = () => enabled;
                return this;
            }

            public Builder SetEnabled(Func<bool> getEnabled)
            {
                _getEnabled = getEnabled;
                return this;
            }

            public Builder SetAction(Action action)
            {
                _action = action;
                return this;
            }

            public Builder SetRefreshEnabled(bool refreshEnabled)
            {
                _refreshEnabled = refreshEnabled;
                return this;
            }

            public Interaction Build()
            {
                return new Interaction
                {
                    GetName = _getName,
                    GetEnabled = _getEnabled,
                    Action = _action,
                    GetTargetName = _targetName,
                    RefreshEnabled = _refreshEnabled,
                };
            }
        }

        public ActionsTypesClass GetActionsTypesClass()
        {
            ActionsTypesClass typesClass = new()
            {
                Action = RefreshEnabled ? Action + _refreshPrompt : Action,
                Name = GetName(),
                Disabled = !GetEnabled(),
            };

            typesClass.TargetName = GetTargetName();

            return typesClass;
        }


        public static List<ActionsTypesClass> GetActionsTypesClassList(List<Interaction> interactions)
        {
            List<ActionsTypesClass> actionsTypesClassList = [];
            string targetName = null;

            foreach (Interaction interaction in interactions)
            {
                if (targetName == null && interaction.GetTargetName != null)
                {
                    targetName = interaction.GetTargetName();
                }

                actionsTypesClassList.Add(interaction.GetActionsTypesClass());
            }

            if (targetName != null && actionsTypesClassList.Any())
            {
                actionsTypesClassList[0].TargetName = targetName;
            }

            return actionsTypesClassList;
        }
    }

}
