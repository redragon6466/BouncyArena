using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class ArenaData
    {
        public string SceneName { get; }

        public Vector3[] TeamOnePos { get; }

        public Vector3[] TeamTwoPos { get;  }

        public Vector3 CameraPos { get;  }

        public Vector3 CameraRot { get;  }

        public AudioClip BackgroundMusic { get; set; }

        public string ArenaName { get; set; }

        public ArenaData(string sceneName, Vector3[] teamOnePos, Vector3[] teamTwoPos, Vector3 cameraPos, Vector3 cameraRot, AudioClip backgroundMusic, string arenaName)
        {
            SceneName = sceneName;
            TeamOnePos = teamOnePos;
            TeamTwoPos = teamTwoPos;
            CameraPos = cameraPos;
            CameraRot = cameraRot;
            BackgroundMusic = backgroundMusic;
            ArenaName = arenaName;

        }
    }
}
