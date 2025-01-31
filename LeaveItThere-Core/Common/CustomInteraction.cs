using EFT.Interactive;
using JetBrains.Annotations;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeaveItThere.Common.InteractionExamples;

namespace LeaveItThere.Common
{
    public abstract class CustomInteraction
    {
        public FakeItem FakeItem { get; private set; }

        /// <summary>
        /// If this constructor is used, FakeItem will not be set. Be sure not to use it unless you use the constructor overload that takes a FakeItem as an argument.
        /// </summary>
        public CustomInteraction() { }

        public CustomInteraction(FakeItem fakeItem)
        {
            FakeItem = fakeItem;
        }

        /// <summary>
        /// Text that shows under the interaction prompt. Only works if it is on the first interaction in the list. GetActionsTypesClassList finds the first TargetName and applies it to the first interaction, but if you are using GetActionsTypesClass you'll have to manually ensure that the first interaction has the desired TargetName set.
        /// </summary>
        public virtual string TargetName { get => null; }

        /// <summary>
        /// If returns true, interaction prompt will be greyed out and not selectable.
        /// </summary>
        public virtual bool Enabled { get => true; }

        /// <summary>
        /// If returns true, the interaction will auto refresh after Action() is called. This will update any names or states of the interaction, but will also reset the selected action to the first one.
        /// </summary>
        public virtual bool AutoPromptRefresh { get => false; }

        /// <summary>
        /// Name of interaction item in prompt.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Interaction callback. Will be called when an interaction is interacted with.
        /// </summary>
        public abstract void OnInteract();

        private static Action _refreshPrompt = new(InteractionHelper.RefreshPrompt);

        public ActionsTypesClass GetActionsTypesClass()
        {
            ActionsTypesClass typesClass = new()
            {
                Action = AutoPromptRefresh ? OnInteract + _refreshPrompt : OnInteract,
                Name = Name,
                Disabled = !Enabled
            };

            typesClass.TargetName = TargetName;

            return typesClass;
        }
        
        public static List<ActionsTypesClass> GetActionsTypesClassList(List<CustomInteraction> interactions)
        {
            List<ActionsTypesClass> actionsTypesClassList = [];
            string targetName = null;

            foreach (CustomInteraction interaction in interactions)
            {
                if (targetName == null && interaction.TargetName != null)
                {
                    targetName = interaction.TargetName;
                }

                actionsTypesClassList.Add(interaction.GetActionsTypesClass());
            }

            if (targetName != null && actionsTypesClassList.Any())
            {
                actionsTypesClassList[0].TargetName = targetName;
            }

            return actionsTypesClassList;
        }

        public class DisabledInteraction(string name) : CustomInteraction
        {
            public override string Name => name;
            public override bool Enabled => false;
            public override void OnInteract() { }
        }
    }

    public class InteractionExamples
    {
        /// <summary>
        /// Simple example that uses a FakeItem input
        /// </summary>
        public class SuperSimpleExample(FakeItem fakeItem) : CustomInteraction(fakeItem)
        {
            public override string Name => "My Interaction Name";
            public override void OnInteract()
            {
                NotificationManagerClass.DisplayMessageNotification($"My FakeItem's name is: {FakeItem.LootItem.Name.Localized()}");
            }
        }


        /// <summary>
        /// This example is good for if you need to initialize another class instance once and store a reference to it in a property or field to be later accessed by the interaction callback
        /// </summary>
        public class ExampleThatRunsCodeOnceWhenCreated : CustomInteraction
        {
            private SomeClass _someClass;

            public ExampleThatRunsCodeOnceWhenCreated(FakeItem fakeItem) : base(fakeItem)
            {
                // code here runs once when the interaction is created (typically every time the item the interaction is being added to is placed)
                _someClass = new SomeClass();
            }

            public override string Name => "Tell Me Length Of My Name";
            public override void OnInteract()
            {
                NotificationManagerClass.DisplayMessageNotification(_someClass.Things);
            }

            public class SomeClass
            {
                public string Things = "Stuff";
            }
        }


        /// <summary>
        /// Shows more potential overrides (note that overriding is unnecessary if the default value is desired, such as Enabled defaulting to true).
        /// Also note that you don't HAVE to give the interaction a FakeItem. But, of course if you do not, then you won't be able to use the FakeItem property.
        /// </summary>
        public class AllTheOtherThingsExample : CustomInteraction
        {
            public override string Name => "My Name";
            public override bool Enabled => false;

            // all overridden properties can also be done like this to have room to run multiple lines of code:
            public override bool AutoPromptRefresh
            {
                get
                {
                    // do code here to determine if AutoRefresh returns true or false
                    return true;
                }
            }

            // they can also take a simpler one line expression like this, it doesn't have to be a raw value
            public override string TargetName => GetString();

            public override void OnInteract()
            {
                NotificationManagerClass.DisplayMessageNotification("Interaction Selected!");
            }

            public string GetString()
            {
                return "A cool string";
            }
        }


        /// <summary>
        /// Ideally in OnFakeItemInitialized callback, add new instances of your interactions to the FakeItem's Actions list.
        /// </summary>
        public class HowToUse
        {
            private void PluginAwakeFunctionOrSimilar()
            {
                LeaveItThereStaticEvents.OnFakeItemInitialized += OnFakeItemInitialized;
            }

            public void OnFakeItemInitialized(FakeItem fakeItem)
            {
                // without a check like this, ALL placed items will get the new interactions
                if (fakeItem.TemplateId != "the item id I am targeting") return;

                fakeItem.Actions.Add(new SuperSimpleExample(fakeItem));
                fakeItem.Actions.Add(new ExampleThatRunsCodeOnceWhenCreated(fakeItem));
                fakeItem.Actions.Add(new AllTheOtherThingsExample());
            }
        }
    }
}
