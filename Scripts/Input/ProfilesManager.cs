using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace HnSF.Input
{
    public class ProfilesManager : MonoBehaviour
    {
        public delegate void ProfileAction(ProfilesManager profilesManager);

        public delegate void ProfileChangeAction(ProfilesManager profilesManager, int index);
#pragma warning disable CS0067 // Event is never used
        public event ProfileAction OnProfileAdded;
        public event ProfileAction onProfileRemoved;
        public event ProfileChangeAction onProfileUpdated;
#pragma warning restore CS0067 // Event is never used

        [SerializeField] protected List<ProfileDefinition> profiles = new List<ProfileDefinition>();
        public ReadOnlyCollection<ProfileDefinition> Profiles => profiles.AsReadOnly();

        public Dictionary<string, int> ProfileNameToIndex = new();

        public ProfileDefinition[] defaultProfiles = new ProfileDefinition[1];

        public InputManager _inputManager;

        public void Init()
        {
            if (!LoadProfiles())
            {
                foreach (var p in defaultProfiles) profiles.Add(p);
                SaveProfiles();
            }
        }

        public void SaveProfiles()
        {
            FileSaveLoadService.SaveFileAsJson("profiles.json", profiles, true);
        }

        public bool LoadProfiles()
        {
            if (!FileSaveLoadService.TryLoadFileFromJson("profiles.json", out List<ProfileDefinition> loadedProfiles))
                return false;
            profiles = loadedProfiles;
            UpdateProfileIndexMappings();
            return true;
        }

        void UpdateProfileIndexMappings()
        {
            ProfileNameToIndex.Clear();
            for (int i = 0; i < profiles.Count; i++)
            {
                ProfileNameToIndex.Add(profiles[i].profileName, i);
            }
        }

        public bool TryCreateProfile(string profileName)
        {
            if (ProfileNameToIndex.ContainsKey(profileName)) return false;
            ProfileDefinition temp = profiles[0];
            temp.undeletable = false;
            temp.profileName = profileName;
            profiles.Add(temp);
            SaveProfiles();
            UpdateProfileIndexMappings();
            return true;
        }

        public void DeleteProfile(string profileName)
        {
            if (!ProfileNameToIndex.ContainsKey(profileName)) return;
            profiles.RemoveAt(ProfileNameToIndex[profileName]);
            SaveProfiles();
            UpdateProfileIndexMappings();
        }

        public bool TryGetProfile(string name, out ProfileDefinition profileDefinition)
        {
            foreach (var p in profiles)
            {
                if (p.profileName.ToLower() == name.ToLower())
                {
                    profileDefinition = p;
                    return true;
                }
            }

            profileDefinition = defaultProfiles[0];
            return false;
        }

        public void ApplyProfile(ProfileDefinition profileDefinition, string profileName)
        {
            if (!ProfileNameToIndex.ContainsKey(profileName)) return;
            var index = ProfileNameToIndex[profileName];
            profiles[index] = profileDefinition;
            onProfileUpdated?.Invoke(this, index);
        }

        public void UpdateProfileBindingOverrides(string bindingOverrides, string profileName)
        {
            if (!ProfileNameToIndex.ContainsKey(profileName)) return;
            int profileIndex = ProfileNameToIndex[profileName];
            var profileDefinition = profiles[profileIndex];

            profileDefinition.overrides = bindingOverrides;

            profiles[profileIndex] = profileDefinition;
            onProfileUpdated?.Invoke(this, profileIndex);

        }
    }
}