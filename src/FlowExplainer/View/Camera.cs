﻿
using System.Numerics;

namespace FlowExplainer
{
    public class Camera : ICamera
    {
        public Vec3 Position = Vec3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public float FovRadians = 65 / 360f * float.Tau;
        public Vec2 RenderTargetSize;
        public CameraProjectionMode ProjectionMode = CameraProjectionMode.Perspective;

        public float NearPlane = .01f;
        public float FarPlane = 2000;
        
        public Camera()
        {

        }


        public Matrix4x4 GetViewMatrix()
            => Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);

        public Matrix4x4 GetProjectionMatrix()
        {
            return ProjectionMode switch
            {
                CameraProjectionMode.Orthographic => Matrix4x4.CreateOrthographic(RenderTargetSize.X/90, RenderTargetSize.Y/90, NearPlane, FarPlane),
                CameraProjectionMode.Perspective => Matrix4x4.CreatePerspectiveFieldOfView(10 / 360f * float.Tau, RenderTargetSize.X / RenderTargetSize.Y, NearPlane, FarPlane),
                _ => throw new Exception($"{nameof(ProjectionMode)} is invalid")
            };
        }
    }
}