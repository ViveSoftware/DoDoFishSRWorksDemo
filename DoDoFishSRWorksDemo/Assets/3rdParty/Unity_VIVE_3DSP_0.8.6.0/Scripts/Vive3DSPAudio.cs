//====================== Copyright 2016-2017, HTC.Corporation. All rights reserved. ======================
/**
*   release version:    0.8.6.0
*   script version:     0.8.6.0
*/

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    public static class Vive3DSPAudio
    {
        public enum OccEngineMode
        {
            BasicOcclusion = 0,
            AdvancedOcclusion,
            RaycastOcclusion
        }

        public enum OccRaycastMode
        {
            MonoOcclusion = 0,                      /**< Mono raycast mode. The ray is casted from one audio source to the listener. */
            BinauralOcclusion                       /**< Binaural mode. Two rays are casted from one audio source to the listener's ear. */
        }

        public enum ChannelType
        {
            Mono = 0,                               /**< A single speaker, typically in front of the user. */
            Stereo
        }

        public struct Vive3DSPQuaternion
        {
            public float x;                         /**< The x-coordinate of the vector part. */
            public float y;                         /**< The y-coordinate of the vector part. */
            public float z;                         /**< The z-coordinate of the vector part. */
            public float w;
        }

        public struct OcclusionVertexPoints
        {
            public float x1;
            public float y1;
            public float z1;
            public float x2;
            public float y2;
            public float z2;
            public float x3;
            public float y3;
            public float z3;
            public float x4;
            public float y4;
            public float z4;
            public float x5;
            public float y5;
            public float z5;
            public float x6;
            public float y6;
            public float z6;
            public float x7;
            public float y7;
            public float z7;
            public float x8;
            public float y8;
            public float z8;
        }

        public enum OccMaterial
        {
            Curtain = 0,
            ThinDoor,
            WoodWall,
            Window,
            HardwoodDoor,
            SeparateWindow,
            HollowCinderConcreteWall,
            SingleLeafBrickWall,
            MetalDoor,
            StoneWall,
            UserDefine
        }

        public enum RoomPlane
        {
            Floor = 0,
            Ceiling,
            LeftWall,
            RightWall,
            FrontWall,
            BackWall
        }

        public enum RoomPlateMaterial
        {
            None = 0,
            Concrete,
            Carpet,
            Wood,
            Glass,
            CoarseConcrete,
            Curtain,
            FiberGlass,
            Foam,
            Sheetrock,
            Plywood,
            Plaster,
            Brick,
            UserDefine
        }

        public enum RoomReverbPreset
        {
            Generic = 0,
            Bathroom,
            Livingroom,
            Church,
            Arena,
            UserDefine
        }

        public enum Ambisonic3dDistanceMode
        {
            RealWorld = 0,
            QuadraticDecay,
            LinearDecay,
            Constant,
        }

        public enum RoomBackgroundAudioType
        {
            None = 0,
            BigRoom,
            SmallRoom,
            AirConditioner,
            Refrigerator,
            PinkNoise,
            BrownNoise,
            WhiteNoise,
            UserDefine,
        }

        public enum HeadsetType
        {
            Generic = 0,
            VIVEPro
        }
        public enum RaycastQuality
        {
            High = 0,
            Median,
            Low
        }
        private static float MaxRayBiasAngle = 30;
        private static float MinRayBiasAngle = 10f;
        private static int RayEmitLayer = 3;
        private static int RayDisperseOrder = 10;
        private static float RayDisperseAngle = 360f / RayDisperseOrder;
        private static float totalRays = RayDisperseOrder * RayEmitLayer + 1;
        public static OccRaycastMode OcclusionMode;
        private static List<Vive3DSPAudioSource> srcList = new List<Vive3DSPAudioSource>();
        private static List<Vive3DSPAudioRoom> roomList = new List<Vive3DSPAudioRoom>();
        private static List<Vive3DSPAudioOcclusion> occList = new List<Vive3DSPAudioOcclusion>();
        private static Vive3DSPAudioListener MainListener = null;
        private static Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        public struct Vive3DSPVersion
        {
            public uint major;
            public uint minor;
            public uint build;
            public uint revision;
        }

        public struct VIVE_3DSP_OCCLUSION_PROPERTY
        {
            public Vector3 position;
            public OcclusionVertexPoints vertex;
            public OccMaterial material;
            public float rhf;
            public float lfratio;
            public float density;
            public float radius;
            public OccEngineMode mode;
        }

        // Source
        public static void CreateSource(Vive3DSPAudioSource srcComponent)
        {
            srcList.Add(srcComponent);
        }
        public static void DestroySource(Vive3DSPAudioSource source)
        {
            srcList.Remove(source);
        }
        public static void ResetSource(Vive3DSPAudioSource source)
        {
            UpdateListenerTransformCallback();
            foreach (var room in roomList)
            {
                UpdateRoomPositionCallback(room);
                UpdateRoomRotationCallback(room);
            }
            UpdateAllOcclusion();
        }
        
        // Listener
        public static void CreateAudioListener(Vive3DSPAudioListener listener)
        {
            MainListener = listener;
            vive_3dsp_create_engine_plugin();
            vive_3dsp_listener_set_headset_plugin(listener.headsetType);
        }
        public static void DestroyAudioListener()
        {
            vive_3dsp_destroy_engine_plugin();
        }

        public static void UpdateAudioListener()
        {
            if (MainListener == null)
                return;

            if (MainListener.occlusionMode == OccRaycastMode.MonoOcclusion)
            {
                vive_3dsp_occlusion_set_channel_num_plugin(1);
                OcclusionMode = OccRaycastMode.MonoOcclusion;
            }
            else
            {
                vive_3dsp_occlusion_set_channel_num_plugin(2);
                OcclusionMode = OccRaycastMode.BinauralOcclusion;
            }

            vive_3dsp_listener_set_gain_plugin(ConvertAmplitudeFromDb(MainListener.globalGain));
        }
        public static void UpdateOcclusionCoverRatio()
        {
            float ratio = 0.0f;

            if (MainListener == null)
                return;

            foreach (var src in srcList)
            {
                if (src.isVirtual)
                    continue;

                foreach (var occ in occList)
                {
                    var StoL_direction = MainListener.transform.position - src.transform.position;

                    if (OcclusionMode == OccRaycastMode.MonoOcclusion)
                    {
                        // Calculate BasicOcclusion in the engine
                        if (occ.occlusionEngine == OccEngineMode.BasicOcclusion ||
                            occ.occlusionEngine == OccEngineMode.AdvancedOcclusion)
                            continue;

                        if (occ.occlusionEngine == OccEngineMode.RaycastOcclusion)
                        {
                            switch (occ.raycastQuality)
                            {
                                case RaycastQuality.High:
                                    MaxRayBiasAngle = 30;
                                    MinRayBiasAngle = 10f;
                                    RayEmitLayer = 2;
                                    RayDisperseOrder = 10;
                                    RayDisperseAngle = 360f / RayDisperseOrder;
                                    totalRays = RayDisperseOrder * RayEmitLayer + 1;
                                    break;
                                case RaycastQuality.Median:
                                    MaxRayBiasAngle = 30;
                                    MinRayBiasAngle = 10f;
                                    RayEmitLayer = 2;
                                    RayDisperseOrder = 6;
                                    RayDisperseAngle = 360f / RayDisperseOrder;
                                    totalRays = RayDisperseOrder * RayEmitLayer + 1;
                                    break;
                                default:
                                    break;
                            }
                            ratio = getRaycastOcclusionRatio(src.transform.position, StoL_direction, occ);
                            src.audioSource.SetSpatializerFloat((int)(Vive3DSPAudioSource.EffectData.MonoCoverOcclusion),
                            BitConverter.ToSingle(BitConverter.GetBytes(occ.OcclusionObject.ToInt32()), 0));
                            src.audioSource.SetSpatializerFloat((int)(Vive3DSPAudioSource.EffectData.MonoCoverRatio), ratio);
                            //Debug.Log("[Mono] ratio = " + ratio);
                        }
                    }
                    else // binaural mode
                    {
                        // Calculate BasicOcclusion in the engine
                        if (occ.occlusionEngine == OccEngineMode.BasicOcclusion ||
                            occ.occlusionEngine == OccEngineMode.AdvancedOcclusion)
                            continue;

                        var direct_head_to_right_ear = 0.1f * MainListener.transform.right / MainListener.transform.right.magnitude;
                        var right_ear_position = MainListener.Position + direct_head_to_right_ear;
                        var left_ear_position = MainListener.Position - direct_head_to_right_ear;

                        var direct_rightear_to_src = src.Position - right_ear_position;
                        var direct_leftear_to_src = src.Position - left_ear_position;

                        float ratio1, ratio2;
                        
                        if (occ.occlusionEngine == OccEngineMode.RaycastOcclusion)
                        {
                            switch (occ.raycastQuality)
                            {
                                case RaycastQuality.High:
                                    MaxRayBiasAngle = 30;
                                    MinRayBiasAngle = 10f;
                                    RayEmitLayer = 2;
                                    RayDisperseOrder = 10;
                                    RayDisperseAngle = 360f / RayDisperseOrder;
                                    totalRays = RayDisperseOrder * RayEmitLayer + 1;
                                    break;
                                case RaycastQuality.Median:
                                    MaxRayBiasAngle = 30;
                                    MinRayBiasAngle = 10f;
                                    RayEmitLayer = 2;
                                    RayDisperseOrder = 6;
                                    RayDisperseAngle = 360f / RayDisperseOrder;
                                    totalRays = RayDisperseOrder * RayEmitLayer + 1;
                                    break;
                                default:
                                    break;
                            }
                            ratio1 = getRaycastOcclusionRatio(src.transform.position, -direct_rightear_to_src, occ);
                            ratio2 = getRaycastOcclusionRatio(src.transform.position, -direct_leftear_to_src, occ);

                            src.audioSource.SetSpatializerFloat((int)(Vive3DSPAudioSource.EffectData.StereoCoverOcclusion),
                            BitConverter.ToSingle(BitConverter.GetBytes(occ.OcclusionObject.ToInt32()), 0));
                            src.audioSource.SetSpatializerFloat((int)(Vive3DSPAudioSource.EffectData.StereoCoverRatioL), ratio2);
                            src.audioSource.SetSpatializerFloat((int)(Vive3DSPAudioSource.EffectData.StereoCoverRatioR), ratio1);
                            //Debug.Log("[Binaural] ratioL = " + ratio2 + ", ratioR = " + ratio1);
                        }
                    }
                }
            }
        }
        public static void CheckIfListenerInRoom(Vive3DSPAudioRoom room)
        {
            if (IsObjectInsideRoom(MainListener.transform.position, room))
            {
                vive_3dsp_listener_set_current_room_plugin(room.RoomObject);
                MainListener.CurrentRoom = room;
                MainListener.RoomList.Add(room);
            }
            else // the listener is outside the room
            {
                MainListener.RoomList.Remove(room);
                if (MainListener.CurrentRoom == room)
                    MainListener.CurrentRoom = null;

                if (MainListener.RoomList.Count > 0)
                {
                    // Set another room
                    foreach (var anotherRoom in MainListener.RoomList)
                    {
                        vive_3dsp_listener_set_current_room_plugin(anotherRoom.RoomObject);
                        if (MainListener.CurrentRoom == null)
                            MainListener.CurrentRoom = anotherRoom;
                        break;
                    }
                }
            }
            if (MainListener.RoomList.Count == 0)
            {
                vive_3dsp_listener_leave_room_plugin();
            }
        }

        public static void UpdateListenerTransformCallback()
        {
            if (MainListener == null)
                return;

            foreach (var room in roomList)
                CheckIfListenerInRoom(room);
        }

        // Occlusion
        public static IntPtr CreateOcclusion(Vive3DSPAudioOcclusion occObj)
        {
            IntPtr occobj = IntPtr.Zero;

            occobj = vive_3dsp_occlusion_create_object_plugin();
            if (occobj != IntPtr.Zero)
            {
                vive_3dsp_occlusion_set_material_plugin(occobj, occObj.OcclusionMaterial);
                occList.Add(occObj);
            }

            return occobj;
        }
        public static void DestroyOcclusion(Vive3DSPAudioOcclusion occObj)
        {
            if (occObj.OcclusionObject != IntPtr.Zero)
            {
                vive_3dsp_occlusion_destroy_object_plugin(occObj.OcclusionObject);
            }
            occList.Remove(occObj);
            occObj.OcclusionObject = IntPtr.Zero;
        }
        public static void EnableOcclusion(IntPtr occ)
        {
            vive_3dsp_occlusion_enable_plugin(occ, true);
        }
        public static void DisableOcclusion(IntPtr occ)
        {
            vive_3dsp_occlusion_enable_plugin(occ, false);
        }
        public static void UpdateOcclusion(Vive3DSPAudioOcclusion occ)
        {
            if (occ.OcclusionObject == IntPtr.Zero)
            {
                return;
            }
            vive_3dsp_occlusion_enable_plugin(occ.OcclusionObject, occ.OcclusionEffect);
            vive_3dsp_occlusion_set_property_plugin(occ.OcclusionObject, occ.OcclusionPorperty);
        }
        private static float getRaycastOcclusionRatio(Vector3 srcPos, Vector3 direction, Vive3DSPAudioOcclusion occ)
        {
            float ratio = 0f;
            int ListenerColliderNumber = 0, SourceColliderNumber = 0;
            var dist = direction.magnitude;
            RaycastHit[] hit;
            hit = Physics.RaycastAll(srcPos, direction, dist);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].transform == occ.transform)
                {
                    SourceColliderNumber++;
                    ListenerColliderNumber++;
                }
            }
            if (occ.raycastQuality == RaycastQuality.Low)
            {
                if (SourceColliderNumber == 0)
                    return 0.0f;
                else
                    return 1.0f;
            }
            var max_direct_forward_up = Quaternion.AngleAxis(MaxRayBiasAngle, Vector3.up) * direction;
            var max_direct_up = max_direct_forward_up - direction;
            var min_direct_forward_up = Quaternion.AngleAxis(MinRayBiasAngle, Vector3.up) * direction;
            var min_direct_up = min_direct_forward_up - direction;

            float maxmin_direct_up_mag_ratio = max_direct_up.magnitude / min_direct_up.magnitude;

            Vector3 rot_up, direct_up, direct_forward_up;
            for (int i = 0; i < RayEmitLayer; ++i)
            {
                direct_up = (i * (maxmin_direct_up_mag_ratio - 1) / RayEmitLayer + 1) * min_direct_up;

                for (int j = 0; j < RayDisperseOrder; ++j)
                {
                    // source part
                    rot_up = Quaternion.AngleAxis(RayDisperseAngle * j, direction) * direct_up;
                    direct_forward_up = direction + rot_up;
                    //Debug.DrawRay(srcPos, rot_up, Color.green);
                    //Debug.DrawRay(srcPos, direct_forward_up, Color.blue);
                    hit = Physics.RaycastAll(srcPos, direct_forward_up, direct_forward_up.magnitude);
                    for (int h = 0; h < hit.Length; h++)
                    {
                        if (hit[h].transform == occ.transform)
                        {
                            ListenerColliderNumber++;
                        }
                    }

                    // listener part
                    direct_forward_up = -direction + rot_up;
                    //Debug.DrawRay(MainListener.transform.position, rot_up, Color.green);
                    //Debug.DrawRay(MainListener.transform.position, direct_forward_up, Color.blue);
                    hit = Physics.RaycastAll(srcPos + direction, direct_forward_up, direct_forward_up.magnitude);
                    for (int h = 0; h < hit.Length; h++)
                    {
                        if (hit[h].transform == occ.transform)
                        {
                            SourceColliderNumber++;
                        }
                    }
                }
            }

            ratio = (float)Math.Max(SourceColliderNumber, ListenerColliderNumber) / totalRays;
            return ratio;
        }

        public static void UpdateAllOcclusion()
        {
            foreach (var occ in occList)
                UpdateOcclusion(occ);
        }
        
        // Room
        public static void CreateRoom(Vive3DSPAudioRoom room)
        {
            room.RoomObject = vive_3dsp_room_create_object_plugin();
            vive_3dsp_room_enable_plugin(room.RoomObject, true);
            roomList.Add(room);
            CheckIfListenerInRoom(room);
        }
        public static void DestroyRoom(Vive3DSPAudioRoom room)
        {
            vive_3dsp_room_destroy_object_plugin(room.RoomObject);

            foreach (var src in srcList)
                src.RoomList.Remove(room);

            MainListener.RoomList.Remove(room);
            roomList.Remove(room);
        }
        public static void UpdateRoom(Vive3DSPAudioRoom roomComponent)
        {
            if (roomComponent.RoomEffect)
            {
                vive_3dsp_room_enable_plugin(roomComponent.RoomObject, true);
            }
            else
            {
                vive_3dsp_room_enable_plugin(roomComponent.RoomObject, false);
            }

            vive_3dsp_room_set_size_plugin(roomComponent.RoomObject, roomComponent.transform.lossyScale);

            if (roomComponent.Ceiling == RoomPlateMaterial.UserDefine)
                vive_3dsp_room_set_reflection_rate_plugin(roomComponent.RoomObject, RoomPlane.Ceiling, roomComponent.CeilingReflectionRate);
            else
                vive_3dsp_room_set_material_plugin(roomComponent.RoomObject, RoomPlane.Ceiling, roomComponent.Ceiling);


            if (roomComponent.FrontWall == RoomPlateMaterial.UserDefine)
                vive_3dsp_room_set_reflection_rate_plugin(roomComponent.RoomObject, RoomPlane.FrontWall, roomComponent.FrontWallReflectionRate);
            else
                vive_3dsp_room_set_material_plugin(roomComponent.RoomObject, RoomPlane.FrontWall, roomComponent.FrontWall);

            if (roomComponent.BackWall == RoomPlateMaterial.UserDefine)
                vive_3dsp_room_set_reflection_rate_plugin(roomComponent.RoomObject, RoomPlane.BackWall, roomComponent.BackWallReflectionRate);
            else
                vive_3dsp_room_set_material_plugin(roomComponent.RoomObject, RoomPlane.BackWall, roomComponent.BackWall);

            if (roomComponent.RightWall == RoomPlateMaterial.UserDefine)
                vive_3dsp_room_set_reflection_rate_plugin(roomComponent.RoomObject, RoomPlane.RightWall, roomComponent.RightWallReflectionRate);
            else
                vive_3dsp_room_set_material_plugin(roomComponent.RoomObject, RoomPlane.RightWall, roomComponent.RightWall);

            if (roomComponent.LeftWall == RoomPlateMaterial.UserDefine)
                vive_3dsp_room_set_reflection_rate_plugin(roomComponent.RoomObject, RoomPlane.LeftWall, roomComponent.LeftWallReflectionRate);
            else
                vive_3dsp_room_set_material_plugin(roomComponent.RoomObject, RoomPlane.LeftWall, roomComponent.LeftWall);

            if (roomComponent.Floor == RoomPlateMaterial.UserDefine)
                vive_3dsp_room_set_reflection_rate_plugin(roomComponent.RoomObject, RoomPlane.Floor, roomComponent.FloorReflectionRate);
            else
                vive_3dsp_room_set_material_plugin(roomComponent.RoomObject, RoomPlane.Floor, roomComponent.Floor);

            vive_3dsp_room_set_reverb_preset_plugin(roomComponent.RoomObject, roomComponent.ReverbPreset);
            vive_3dsp_room_set_size_plugin(roomComponent.RoomObject, Vector3.Scale(roomComponent.transform.lossyScale, roomComponent.Size));
            vive_3dsp_room_setReflectionLevel_plugin(roomComponent.RoomObject, roomComponent.reflectionLevel);
            vive_3dsp_room_setReverbLevel_plugin(roomComponent.RoomObject, roomComponent.reverbLevel);

        }
        public static void UpdateRoomPositionCallback(Vive3DSPAudioRoom roomComponent)
        {
            if (roomComponent.RoomObject == IntPtr.Zero)
                return;
            vive_3dsp_room_set_position_plugin(roomComponent.RoomObject, roomComponent.Position);
            CheckIfListenerInRoom(roomComponent);
        }
        public static void UpdateRoomRotationCallback(Vive3DSPAudioRoom roomComponent)
        {
            if (roomComponent.RoomObject == IntPtr.Zero)
                return;
            vive_3dsp_room_set_rotation_plugin(roomComponent.RoomObject, roomComponent.transform.rotation);
            CheckIfListenerInRoom(roomComponent);
        }

        public static bool IsObjectInsideRoom(Vector3 object_pos, Vive3DSPAudioRoom room)
        {
            bool isInside = false;

            Vector3 relativePosition = object_pos - room.transform.position;
            Quaternion rotationInverse = Quaternion.Inverse(room.transform.rotation);

            bounds.size = Vector3.Scale(room.transform.lossyScale, room.Size);
            isInside = bounds.Contains(rotationInverse * relativePosition);

            return isInside;
        }
        public static float[] GetBGAudioData(int file_id)
        {
            float[] buffer = new float[48000];
            IntPtr bg_data = vive_3dsp_room_get_bgaudio_plugin(file_id);
            if (bg_data == IntPtr.Zero)
                return null;
            Marshal.Copy(bg_data, buffer, 0, buffer.Length);
            vive_3dsp_room_free_bgaudio_plugin(bg_data);
            return buffer;
        }
        public static float ConvertAmplitudeFromDb(float db)
        {
            return Mathf.Pow(10.0f, 0.05f * db);
        }
        private const string pluginName = "audioplugin_vive3dsp";


        // Engine handlers
        [DllImport(pluginName)]
        private static extern int vive_3dsp_create_engine_plugin();
        [DllImport(pluginName)]
        private static extern int vive_3dsp_destroy_engine_plugin();
        
        // Listener handlers
        [DllImport(pluginName)]
        private static extern int vive_3dsp_listener_set_gain_plugin(float gain);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_listener_set_current_room_plugin(IntPtr room);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_listener_leave_room_plugin();
        [DllImport(pluginName)]
        private static extern int vive_3dsp_listener_set_headset_plugin(HeadsetType mode);
        
        // Room
        [DllImport(pluginName)]
        private static extern IntPtr vive_3dsp_room_create_object_plugin();
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_destroy_object_plugin(IntPtr room);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_set_size_plugin(IntPtr room, Vector3 size);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_set_position_plugin(IntPtr room, Vector3 pos);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_set_rotation_plugin(IntPtr room, Quaternion rot);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_set_material_plugin(IntPtr room, RoomPlane room_plane, RoomPlateMaterial room_material);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_set_reverb_preset_plugin(IntPtr room, RoomReverbPreset room_reverb_preset);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_set_reflection_rate_plugin(IntPtr room, RoomPlane room_plane, float rate);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_enable_plugin(IntPtr room, bool enable);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_setReflectionLevel_plugin(IntPtr room, float level_dB);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_setReverbLevel_plugin(IntPtr room, float level_dB);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_set_bgaudio_plugin(IntPtr room, int audio_id);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_set_bgaudio_volume_plugin(IntPtr room, float level_dB);
        [DllImport(pluginName)]
        private static extern IntPtr vive_3dsp_room_get_bgaudio_plugin(int audio_id);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_room_free_bgaudio_plugin(IntPtr pBGAudio);

        // Occlusion handlers
        [DllImport(pluginName)]
        private static extern IntPtr vive_3dsp_occlusion_create_object_plugin();
        [DllImport(pluginName)]
        private static extern int vive_3dsp_occlusion_destroy_object_plugin(IntPtr occ);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_occlusion_enable_plugin(IntPtr occ, bool enable);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_occlusion_set_material_plugin(IntPtr occ, OccMaterial material);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_occlusion_set_material_RHF_plugin(IntPtr occ, float rhf);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_occlusion_set_material_LF_ratio_plugin(IntPtr occ, float LF_ratio);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_occlusion_set_material_HS_params_plugin(IntPtr occ, float gain, float freq, float Q);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_occlusion_set_channel_num_plugin(int chnNum);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_occlusion_set_depth_ratio_plugin(IntPtr occ, float depthRatio);
        [DllImport(pluginName)]
        private static extern int vive_3dsp_occlusion_set_property_plugin(IntPtr occlusion, VIVE_3DSP_OCCLUSION_PROPERTY prop);

        [DllImport(pluginName)]
        private static extern int vive_3dsp_get_version_plugin(IntPtr ver);
    }
}
