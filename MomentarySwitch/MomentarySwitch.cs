﻿using KSerialization;
using STRINGS;

namespace Psyko.MomentarySwitch
{
    
    [SerializationConfig(MemberSerialization.OptIn)]
    public class MomentarySwitch: Switch, IPlayerControlledToggle, ISingleSliderControl, ISim200ms
    {
        private bool wasOn;
        [Serialize] private float delayAmount = 1f;
        [Serialize] private float timeRemaining;
        [Serialize] private bool normallyOpen = true;
        public event System.Action onStateChange;
        
        // public float DelayAmount { get; set; }

        public bool NormallyOpen
        {
            get => this.normallyOpen;
            set
            {
                if (this.normallyOpen != value)
                {
                    Toggle();
                }
                this.normallyOpen = value;
            }
        }

        protected override void Toggle()
        {
            base.Toggle();
            onStateChange.Signal();
            this.timeRemaining = this.delayAmount;
            this.UpdateVisualization();
            this.UpdateLogicCircuit();
        }

        public void ToggledByPlayer()
        {
            this.Toggle();
        }

        public bool ToggledOn()
        {
            return this.switchedOn;
        }

        public KSelectable GetSelectable()
        {
            return GetComponent<KSelectable>();
        }

        public bool ToggleRequested { get; set; }
        
        public void Sim200ms(float dt)
        {
            if (IsSwitchedOn)
            {
                timeRemaining -= 0.2f;
                if (timeRemaining <= 0)
                {
                    OnDelay();
                    return;
                }
            }
            
            if (!ToggleRequested)
                return;
            
            Toggle();
            ToggleRequested = false;
            GetSelectable().SetStatusItem(Db.Get().StatusItemCategories.Main, null);
        }
        
        private void UpdateLogicCircuit()
        {
            GetComponent<LogicPorts>().SendSignal(LogicSwitch.PORT_ID, this.switchedOn ? 1 : 0);
        }
        
        private void UpdateVisualization()
        {
            KBatchedAnimController component = this.GetComponent<KBatchedAnimController>();
            if (this.wasOn != this.switchedOn)
            {
                component.Play((HashedString) (this.switchedOn ? "on_pre" : "on_pst"));
                component.Queue((HashedString) (this.switchedOn ? "on" : "off"));
            }
            this.wasOn = this.switchedOn;
        }
        
        protected override void UpdateSwitchStatus()
        {
            StatusItem status = this.switchedOn ? Db.Get().BuildingStatusItems.LogicSwitchStatusActive : Db.Get().BuildingStatusItems.LogicSwitchStatusInactive;
            this.GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Power, status);
        }
        
        private void OnDelay()
        {
            this.timeRemaining = 0;
            this.Toggle();
        }
        
        public string SideScreenTitleKey => "STRINGS.BUILDINGS.PREFABS.MOMENTARYSWITCH.SIDESCREEN_TITLE";

        public string SliderTitleKey => "STRINGS.BUILDINGS.PREFABS.MOMENTARYSWITCH.SIDESCREEN_TITLE";

        public string SliderUnits => (string) UI.UNITSUFFIXES.SECOND;
        
        public int SliderDecimalPlaces(int index) => 1;
        
        public float GetSliderMin(int index) => 0.1f;

        public float GetSliderMax(int index) => 10f;

        public float GetSliderValue(int index) => this.delayAmount;

        public void SetSliderValue(float value, int index) => this.delayAmount = value;

        public string GetSliderTooltipKey(int index) => "STRINGS.BUILDINGS.PREFABS.MOMENTARYSWITCH.SIDESCREEN_DURATION_TOOLTIP";

        string ISliderControl.GetSliderTooltip(int index) => string.Format((string) Strings.Get(GetSliderTooltipKey(index)), (object) this.delayAmount);

        
    }
}