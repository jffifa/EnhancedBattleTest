﻿using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace EnhancedBattleTest
{
    public class DisableDeathLogic : MissionLogic
    {
        private BattleConfigBase _config;

        public DisableDeathLogic(BattleConfigBase config)
        {
            _config = config;
        }
        public override void AfterStart()
        {
            base.AfterStart();
            SetDisableDeath(_config.disableDeath);
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this.Mission.InputManager.IsKeyPressed(InputKey.F11))
            {
                this._config.disableDeath = !this._config.disableDeath;
                SetDisableDeath(this._config.disableDeath);
            }
        }

        public void SetDisableDeath(bool disableDeath)
        {
            Mission.DisableDying = disableDeath;
            PrintDeathStatus(disableDeath);
        }

        private void PrintDeathStatus(bool disableDeath)
        {
            Utility.DisplayLocalizedText(disableDeath ? "str_death_disabled" : "str_death_enabled");
        }
    }
}
