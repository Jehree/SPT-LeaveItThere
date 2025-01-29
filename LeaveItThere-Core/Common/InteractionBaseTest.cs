using JetBrains.Annotations;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveItThere.Common
{
    public class InteractionBaseTest
    {
        public virtual string Name { get => "Default Interaction Name"; }
        public virtual string TargetName { get => null; }
        public virtual bool Enabled { get => true; }
        public virtual bool AutoRefresh { get => false; }
        private static Action _refreshPrompt = new(InteractionHelper.RefreshPrompt);

        public virtual void Action() { }

        public ActionsTypesClass GetActionsTypesClass()
        {
            ActionsTypesClass typesClass = new()
            {
                Action = AutoRefresh ? Action + _refreshPrompt : Action,
                Name = Name,
                Disabled = !Enabled
            };

            typesClass.TargetName = TargetName;

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

    public class ReclaimInteraction : InteractionBaseTest
    {
        public FakeItem FakeItem { get; private set; }

        public ReclaimInteraction(FakeItem fakeItem)
        {
            FakeItem = fakeItem;
        }

        public override string Name {
            get
            {
                return FakeItem.AddonFlags.ReclaimInteractionDisabled
                    ? "Reclaim: Disabled"
                    : "Reclaim";
            }
        }

        public override bool Enabled => !FakeItem.AddonFlags.ReclaimInteractionDisabled;

        public override void Action()
        {
            FikaInterface.SendPlacedStateChangedPacket(FakeItem, false);
            FakeItem.Reclaim();
            FakeItem.ReclaimPlayerFeedback();
        }
    }
}
