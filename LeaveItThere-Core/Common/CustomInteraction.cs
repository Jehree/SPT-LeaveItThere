﻿using System;
using System.Collections.Generic;

namespace LeaveItThere.Common
{
    public class CustomInteraction
    {
        public Action Action;
        public Func<string> GetName { get; private set; }
        public Func<bool> GetDisabled { get; private set; }
        public string TargetName { get; private set; }

        /// <summary>
        /// Custom Interaction Action to be added to an interactable prompt<br/>
        /// </summary>
        /// <remarks>
        /// name can be a <see cref="string"/> or a <see cref="Func{}"/> that returns <see cref="string"/><br/>
        /// disabled can be a <see cref="bool"/> or a <see cref="Func{}"/> that returns <see cref="bool"/><br/>
        /// </remarks>
        public CustomInteraction(string name, bool disabled, Action action, string targetName = null) // simple static name and disabled
        {
            GetName = () => name;
            GetDisabled = () => disabled;
            Action = action;
            TargetName = targetName;
        }

        public CustomInteraction(Func<string> getName, Func<bool> getDisabled, Action action, string targetName = null) // dynamic simple and static
        {
            GetName = getName;
            GetDisabled = getDisabled;
            Action = action;
            TargetName = targetName;
        }

        public CustomInteraction(string name, Func<bool> getDisabled, Action action, string targetName = null) // simple name, dynamic disabled
        {
            GetName = () => name;
            GetDisabled = getDisabled;
            Action = action;
            TargetName = targetName;
        }

        public CustomInteraction(Func<string> getName, bool disabled, Action action, string targetName = null) // dynamic name, simple disabled
        {
            GetName = getName;
            GetDisabled = () => disabled;
            Action = action;
            TargetName = targetName;
        }

        public ActionsTypesClass GetActionsTypesClass()
        {
            var typesClass = new ActionsTypesClass
            {
                Action = Action,
                Name = GetName(),
                Disabled = GetDisabled(),
            };

            if (TargetName != null)
            {
                typesClass.TargetName = (string)TargetName;
            }

            return typesClass;
        }

        public static List<ActionsTypesClass> GetActionsTypesClassList(List<CustomInteraction> CustomInteractionActionList)
        {
            List<ActionsTypesClass> actionsTypesClassList = new List<ActionsTypesClass>();

            foreach (CustomInteraction CustomInteractionAction in CustomInteractionActionList)
            {
                actionsTypesClassList.Add(CustomInteractionAction.GetActionsTypesClass());
            }

            return actionsTypesClassList;
        }
    }

}
