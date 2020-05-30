using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EnhancedBattleTest.src;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using Module = TaleWorlds.MountAndBlade.Module;

namespace EnhancedBattleTest
{
    public class EnhancedBattleTestSubModule : MBSubModuleBase
    {
        private readonly Harmony harmony = new Harmony("MissionAgentSpawnLogicForMpPatch");
        private readonly MethodInfo original = typeof(MissionAgentSpawnLogic).GetNestedType("MissionSide", BindingFlags.NonPublic).GetMethod("SpawnTroops", BindingFlags.Instance | BindingFlags.Public);
        private readonly MethodInfo prefix = typeof(HarmonyPatchForSpawnLogic).GetMethod("SpawnTroops");
        public static EnhancedBattleTestSubModule Instance { get; private set; }

        public static string ModuleFolderName = "EnhancedBattleTest";

        public static string ModuleFolderPath = Path.Combine(BasePath.Name, "Modules", ModuleFolderName);

        public static bool IsMultiplayer;

        public CharacterSelectionLayer CharacterSelectionLayer;

        public event Action<CharacterSelectionData> OnSelectCharacter;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            EnhancedBattleTestSubModule.Instance = this;
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("EBTMultiplayerTest",
                new TextObject("{=EnhancedBattleTest_multiplayerbattleoption}Multiplayer Battle Test"), 3,
                () =>
                {
                    IsMultiplayer = true;
                    MBGameManager.StartNewGame(new EnhancedBattleTestGameManager<MultiplayerGame>());
                }, false));
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("EBTSingleplayerTest",
                new TextObject("{=EnhancedBattleTest_singleplayerbattleoption}Singleplayer Battle Test"), 3,
                () =>
                {
                    IsMultiplayer = false;
                    MBGameManager.StartNewGame(new EnhancedBattleTestSingleplayerGameManager());
                }, false));
        }

        protected override void OnSubModuleUnloaded()
        {
            EnhancedBattleTestSubModule.Instance = (EnhancedBattleTestSubModule)null;
            base.OnSubModuleUnloaded();
        }

        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);

            if (game.GameType is MultiplayerGame || game.GameType is Campaign)
            {
                var collection = CharacterCollection.Create(IsMultiplayer);
                collection.Initialize();
                CharacterSelectionLayer = new CharacterSelectionLayer();
                CharacterSelectionLayer.Initialize(collection, IsMultiplayer);
                ApplyHarmonyPatch();
            }
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);

            if (game.GameType is MultiplayerGame || game.GameType is Campaign)
            {
                CharacterSelectionLayer.OnFinalize();
                Unpatch();
            }
        }

        public void SelectCharacter(CharacterSelectionData data)
        {
            OnSelectCharacter?.Invoke(data);
        }

        private void ApplyHarmonyPatch()
        {
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        private void Unpatch()
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }
}