using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using R2API;
using static R2API.DirectorAPI;
using RoR2;
using UnityEngine.AddressableAssets;

namespace FabricatorStandalone
{
    public static class Spawnlists
    {
        public static void Init()
        {
            //get spawncards
            SpawnCards.Init();
        }
        public static void AddMonsterCardToSpawnlist(DirectorCardCategorySelection categorySelection, DirectorCard directorCard, MonsterCategory monsterCategory)
        {
            categorySelection.AddCard((int)monsterCategory, directorCard);
        }
    }

    public static class SpawnCards
    {
        public static bool initialized = false;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            DirectorCards.Init();
        }
    }
    public static class DirectorCards
    {
        public static bool initialized = false;

        public static bool logCardInfo = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;

        }

        public static DirectorCard BuildDirectorCard(CharacterSpawnCard spawnCard)
        {
            return BuildDirectorCard(spawnCard, 1, 0, DirectorCore.MonsterSpawnDistance.Standard);
        }

        public static DirectorCard BuildDirectorCard(CharacterSpawnCard spawnCard, int weight, int minStages, DirectorCore.MonsterSpawnDistance spawnDistance)
        {
            DirectorCard dc = new DirectorCard
            {
                spawnCard = spawnCard,
                selectionWeight = weight,
                preventOverhead = false,
                minimumStageCompletions = minStages,
                spawnDistance = spawnDistance
            };
            return dc;
        }
        public static DirectorCard BuildDirectorCard(InteractableSpawnCard spawnCard, int weight, int minStages)
        {
            DirectorCard dc = new DirectorCard
            {
                spawnCard = spawnCard,
                selectionWeight = weight,
                preventOverhead = false,
                minimumStageCompletions = minStages
            };
            return dc;
        }
    }
}
