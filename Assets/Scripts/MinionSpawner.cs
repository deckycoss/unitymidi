using System;
using UnityEngine;
using System.Collections;
using CSharpSynth.Midi;
using Object=UnityEngine.Object;

public class MinionSpawner : MonoBehaviour
{
    private Transform minionContainer;
    public Transform spawnPoint;
	public float radius = 5f;
	
    public MidiPlayer midiPlayer;
    public int noteOnsBetweenEachSpawn;

    public GameObject[] prefabsToSpawn;

    public MidionLevel level;

    public int levelTimelineIndex { get; private set; }
    private float startTime = 0f;

    public Vector3 SpawnPosition { get { return spawnPoint != null ? spawnPoint.position : Vector3.zero; } }

    void Start()
    {
        if (CrossSceneComm.levelToPlay == null) return;

        levelTimelineIndex = 0;
        midiPlayer.OnNoteOn += ProcessNoteOn;
        midiPlayer.OnNoteOff += ProcessNoteOff;
        midiPlayer.OnOtherMidiEvent += ProcessOtherMidiMessage;
        if (CrossSceneComm.levelToPlay != null) {
            level = CrossSceneComm.levelToPlay;
        }
        minionContainer = new GameObject("MinionContainer").transform;
        prefabsToSpawn = CrossSceneComm.levelToPlay.channelPrefabMap;
        Begin();
    }

    private void Begin()
    {
        if (level == null) {
            Debug.LogError("Need to give MinionSpawner a MidionLevel Object!");
            return;
        }
        startTime = Time.time;
        midiPlayer.Play(level.midiFile);
    }

    int noteOnCounter = 0;
    private void ProcessNoteOn(int channel, int note, int velocity)
    {
		if (level == null) return;

        var spawnPosition = new Vector3(SpawnPosition.x, UnityEngine.Random.Range(-radius, radius), SpawnPosition.z);

        // Try to advance through the timeline of active channels
        if (level.timeline.Length - 1 > levelTimelineIndex) {
            if (level.timeline[levelTimelineIndex + 1].time <= Time.time - startTime) {
                levelTimelineIndex++;
            }
        }
        var activeChannels = level.timeline[levelTimelineIndex].activeChannels;

        //if channel is not active/present in activeChannels, don't process
        if (channel < activeChannels.Length && !activeChannels[channel]) return;
		
        if (noteOnsBetweenEachSpawn != 0 && noteOnCounter % noteOnsBetweenEachSpawn == 0) {
            if (prefabsToSpawn.Length >= channel && prefabsToSpawn[channel] != null)
                (Object.Instantiate(prefabsToSpawn[channel], spawnPosition, Quaternion.identity) as GameObject).transform.parent = minionContainer;
        }
        noteOnCounter++;
    }

    private void ProcessNoteOff(int channel, int note)
    {
    }

    private void ProcessOtherMidiMessage(MidiEvent midiEvent)
    {
    }
}
