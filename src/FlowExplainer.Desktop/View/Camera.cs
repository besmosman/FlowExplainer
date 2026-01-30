using System.Numerics;

namespace FlowExplainer
{
    public class Camera : ICamera
    {
        public Vec3 Position = Vec3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public double FovRadians = 65 / 360f * double.Tau;
        public Vec2 RenderTargetSize;
        public CameraProjectionMode ProjectionMode = CameraProjectionMode.Perspective;

        public double NearPlane = .01f;
        public double FarPlane = 2000;

        public Camera()
        {

        }


        public Matrix4x4 GetViewMatrix()
            => Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);

        public Matrix4x4 GetProjectionMatrix()
        {
            return ProjectionMode switch
            {
                CameraProjectionMode.Orthographic => Matrix4x4.CreateOrthographic((float)RenderTargetSize.X / 90, (float)RenderTargetSize.Y / 90, (float)NearPlane, (float)FarPlane),
                CameraProjectionMode.Perspective => Matrix4x4.CreatePerspectiveFieldOfView((float)(20 / 360f * double.Tau), (float)RenderTargetSize.X / (float)RenderTargetSize.Y, (float)NearPlane, (float)FarPlane),
                _ => throw new Exception($"{nameof(ProjectionMode)} is invalid")
            };
        }
    }
}