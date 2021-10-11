using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore
{
    public class WorldView
    {
        public bool DisableAllInput = false;

        public Transform CameraTransform = Transform.Default;
        public Transform CameraOrigin = Transform.Default;
        public Transform CameraPositionDefault = Transform.Default;
        public float OrbitCamDistance = 12;
        public float ModelHeight_ForOrbitCam = 1;
        public float ModelDepth_ForOrbitCam = 1;
        public Vector3 ModelCenter_ForOrbitCam = Vector3.Zero;
        public Vector3 OrbitCamCenter = new Vector3(0, 0.5f, 0);

        private Rectangle BoundingRect;

        public Matrix4x4 WorldMatrixMOD = Matrix4x4.Identity;

        public WorldView(Rectangle bounds)
        {
            BoundingRect = bounds;
        }

        public void UpdateBounds(Rectangle bounds)
        {
            BoundingRect = bounds;
        }

        public Vector3 LightRotation = Vector3.Zero;
        public Vector3 LightDirectionVector => 
            Vector3.Transform(Vector3.UnitX,
            Matrix4x4.CreateRotationY(LightRotation.Y)
            * Matrix4x4.CreateRotationZ(LightRotation.Z)
            * Matrix4x4.CreateRotationX(LightRotation.X)
            );

        public Matrix4x4 MatrixWorld;
        public Matrix4x4 MatrixProjection;

        public float FieldOfView = 43;
        public float NearClipDistance = 0.1f;
        public float FarClipDistance = 2000;
        public float SHITTY_CAM_ZOOM_MIN_DIST = 0.2f;
        public float CameraTurnSpeedGamepad = 1.5f * 0.1f;
        public float CameraTurnSpeedMouse = 1.5f * 0.25f;

        public float CameraMoveSpeed = 20.0f;
        public float CameraMoveSpeedFast = 200.0f;
        public float CameraMoveSpeedSlow = 1.0f;

        public static readonly Vector3 CameraDefaultPos = new Vector3(0, 0.25f, -5);
        public static readonly Vector3 CameraDefaultRot = new Vector3(0, 0, 0);

        public void ResetCameraLocation()
        {
            CameraTransform.Position = CameraDefaultPos;
            CameraTransform.EulerRotation = CameraDefaultRot;
        }

        public void LookAtTransform(Transform t)
        {
            var newLookDir = Vector3.Normalize(t.Position - (CameraTransform.Position));
            var eu = CameraTransform.EulerRotation;
            eu.Y = (float)Math.Atan2(-newLookDir.X, newLookDir.Z);
            eu.X = (float)Math.Asin(newLookDir.Y);
            eu.Z = 0;
            CameraTransform.EulerRotation = eu;
        }

        public void GoToTransformAndLookAtIt(Transform t, float distance)
        {
            var positionOffset = Vector3.Transform(Vector3.UnitX, t.RotationMatrix) * distance;
            CameraTransform.Position = t.Position + positionOffset;
            LookAtTransform(t);
        }

        public float GetDistanceSquaredFromCamera(Transform t)
        {
            return (t.Position - GetCameraPhysicalLocation().Position).LengthSquared();
        }

        public Vector3 ROUGH_GetPointOnFloor(Vector3 pos, Vector3 dir, float stepDist)
        {
            Vector3 result = pos;
            Vector3 nDir = Vector3.Normalize(dir);
            while (result.Y > 0)
            {
                if (result.Y >= 1)
                    result += nDir * 1;
                else
                    result += nDir * stepDist;
            }
            result.Y = 0;
            return result;
        }

        public Transform GetSpawnPointFromScreenPos(Vector2 screenPos, float distance, bool faceBackwards, bool lockPitch, bool alignToFloor)
        {
            var result = Transform.Default;
            return result;
        }

        public Transform GetCameraPhysicalLocation()
        {
            var result = Transform.Default;
            return result;
        }

        public void SetCameraLocation(Vector3 pos, Vector3 rot)
        {
            CameraTransform.Position = pos;
            CameraTransform.EulerRotation = rot;
        }

        public void UpdateMatrices()
        {
            MatrixWorld = Matrix4x4.CreateRotationY(Utils.Pi)
                * Matrix4x4.CreateTranslation(0, 0, 0)
                * Matrix4x4.CreateScale(-1, 1, 1)
                // * Matrix.Invert(CameraOrigin.ViewMatrix)
                ;

        }

        public void MoveCamera(float x, float y, float z, float speed)
        {
            CameraTransform.Position += Vector3.Transform(new Vector3(x, y, z),
                CameraTransform.Rotation
                ) * speed;
        }

        public void UpdateOrbitCameraCenter()//		Set the OrbitCamCenter to be the position OrbitCamDistance in front of the camera
        {
            OrbitCamCenter = CameraTransform.Position + Vector3.Transform(new Vector3(0, 0, OrbitCamDistance),
                CameraTransform.Rotation);
        }

        public void RotateOrbitCamera(float h, float v, float speed)
        {
            var eu = CameraTransform.EulerRotation;
            eu.Y -= h * speed;
            eu.X = Math.Clamp(eu.X + v * speed , -1.572f , 1.5715f);//		negative is looking up
            CameraTransform.EulerRotation = eu;
            CameraTransform.Position = OrbitCamCenter - Vector3.Transform(new Vector3(0, 0, OrbitCamDistance),
                CameraTransform.Rotation);
        }
        /*
        public void MoveCamera_OrbitCenterPoint(float x, float y, float z, float speed)
        {
            OrbitCamCenter += (Vector3.Transform(new Vector3(x, y, z),
                Matrix4x4.CreateRotationX(-CameraTransform.EulerRotation.X)
                * Matrix4x4.CreateRotationY(-CameraTransform.EulerRotation.Y)
                * Matrix4x4.CreateRotationZ(-CameraTransform.EulerRotation.Z)
                ) * speed) * (OrbitCamDistance * OrbitCamDistance) * 0.5f;
        }*/

        public void PointCameraToLocation(Vector3 location)
        {
            var newLookDir = Vector3.Normalize(location - (CameraTransform.Position));
            var eu = CameraTransform.EulerRotation;
            eu.Y = (float)Math.Atan2(newLookDir.X, newLookDir.Z);
            eu.X = (float)Math.Asin(newLookDir.Y);
            eu.Z = 0;
            CameraTransform.EulerRotation = eu;
        }


        private Vector2 mousePos = Vector2.Zero;
        private Vector2 oldMouse = Vector2.Zero;
       // private int oldWheel = 0;
        private bool currentMouseClickL = false;
        private bool currentMouseClickR = false;
        private bool currentMouseClickM = false;
        private bool currentMouseClickStartedInWindow = false;
        private bool oldMouseClickL = false;
        private bool oldMouseClickR = false;
        private bool lastFrameMouseClickM = false;
        private bool shiftWasHeldBeforeClickM = false;
        private MouseClickType currentClickType = MouseClickType.None;
        private MouseClickType oldClickType = MouseClickType.None;
        //軌道カムトグルキー押下
       // bool oldOrbitCamToggleKeyPressed = false;
        //非常に悪いカメラピッチ制限    ファトキャット
        const float SHITTY_CAM_PITCH_LIMIT_FATCAT = 0.999f;
        //非常に悪いカメラピッチ制限リミッタ    ファトキャット
        const float SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP = 0.999f;

        private bool oldResetKeyPressed = false;

        private float GetGamepadTriggerDeadzone(float t, float d)
        {
            if (t < d)
                return 0;
            else if (t >= 1)
                return 0;

            return (t - d) * (1.0f / (1.0f - d));
        }

        public enum MouseClickType
        {
            None,
            Left,
            Right,
            Middle,
            Extra1,
            Extra2,
        }

        private bool MousePressed = false;
        private Vector2 MousePressedPos = new Vector2();

        public bool UpdateInput(Sdl2Window window, float dt)
        {
            if (DisableAllInput)
            {
                //oldWheel = Mouse.GetState(game.Window).ScrollWheelValue;
                return false;
            }

            float clampedLerpF = Utils.Clamp(30 * dt, 0, 1);

            mousePos = new Vector2(Utils.Lerp(oldMouse.X, InputTracker.MousePosition.X, clampedLerpF),
                Utils.Lerp(oldMouse.Y, InputTracker.MousePosition.Y, clampedLerpF));



            //KeyboardState keyboard = DBG.EnableKeyboardInput ? Keyboard.GetState() : DBG.DisabledKeyboardState;
            //int currentWheel = mouse.ScrollWheelValue;

            //bool mouseInWindow = MapStudio.Active && mousePos.X >= game.ClientBounds.Left && mousePos.X < game.ClientBounds.Right && mousePos.Y > game.ClientBounds.Top && mousePos.Y < game.ClientBounds.Bottom;

            currentClickType = MouseClickType.None;

            if (InputTracker.GetMouseButton(Veldrid.MouseButton.Left))
                currentClickType = MouseClickType.Left;
            else if (InputTracker.GetMouseButton(Veldrid.MouseButton.Right))
                currentClickType = MouseClickType.Right;
            else if (InputTracker.GetMouseButton(Veldrid.MouseButton.Middle))
                currentClickType = MouseClickType.Middle;
            else if (InputTracker.GetMouseButton(Veldrid.MouseButton.Button1))
                currentClickType = MouseClickType.Extra1;
            else if (InputTracker.GetMouseButton(Veldrid.MouseButton.Button2))
                currentClickType = MouseClickType.Extra2;
            else
                currentClickType = MouseClickType.None;
            
            //		Zoom controls (This code must be above the return or zooming will only work while holding a mouse button)
            int mouseWheel = Math.Sign(InputTracker.GetMouseWheelDelta());
            if (mouseWheel != 0)
            {
                //		Multiplying the change by OrbitCamDistance will make zooming finer when close to the OrbitCamCenter
                OrbitCamDistance = Math.Min(Math.Max(OrbitCamDistance -0.15f*mouseWheel*OrbitCamDistance , SHITTY_CAM_ZOOM_MIN_DIST) , FarClipDistance);
                RotateOrbitCamera(0, 0, 0);//		Rotate nowhere to update camera distance to OrbitCamCenter
            }

            currentMouseClickL = currentClickType == MouseClickType.Left;
            currentMouseClickR = currentClickType == MouseClickType.Right;
            currentMouseClickM = currentClickType == MouseClickType.Middle;

            if (currentClickType != MouseClickType.None && oldClickType == MouseClickType.None)
                currentMouseClickStartedInWindow = true;

            if (currentClickType == MouseClickType.None)
            {
                // If nothing is pressed, just dont bother lerping
                //mousePos = new Vector2(mouse.X, mouse.Y);
                if (MousePressed)
                {
                    mousePos = InputTracker.MousePosition;
                    Sdl2Native.SDL_WarpMouseInWindow(window.SdlWindowHandle, (int)MousePressedPos.X, (int)MousePressedPos.Y);
                    Sdl2Native.SDL_SetWindowGrab(window.SdlWindowHandle, false);
                    Sdl2Native.SDL_ShowCursor(1);
                    MousePressed = false;
                }

                lastFrameMouseClickM = false;

                return false;
            }

            bool isSpeedupKeyPressed = InputTracker.GetKey(Veldrid.Key.LShift) || InputTracker.GetKey(Veldrid.Key.RShift);
            bool isSlowdownKeyPressed = InputTracker.GetKey(Veldrid.Key.LControl) || InputTracker.GetKey(Veldrid.Key.RControl);
            bool isResetKeyPressed = InputTracker.GetKey(Veldrid.Key.R);
            bool isMoveLightKeyPressed = InputTracker.GetKey(Veldrid.Key.Space);
            bool isPointCamAtObjectKeyPressed = false;// keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T);


            if (!currentMouseClickStartedInWindow)
            {
                oldMouse = mousePos;

                var euler = CameraTransform.EulerRotation;
                euler.X = Utils.Clamp(CameraTransform.EulerRotation.X, -Utils.PiOver2, Utils.PiOver2);
                CameraTransform.EulerRotation = euler;

                LightRotation.X = Utils.Clamp(LightRotation.X, -Utils.PiOver2, Utils.PiOver2);

                oldClickType = currentClickType;

                oldMouseClickL = currentMouseClickL;
                oldMouseClickR = currentMouseClickR;
                lastFrameMouseClickM = currentMouseClickM;

                return true;
            }


            if (isResetKeyPressed && !oldResetKeyPressed)
            {
                ResetCameraLocation();
            }

            oldResetKeyPressed = isResetKeyPressed;

            if (isPointCamAtObjectKeyPressed)
            {
                PointCameraToLocation(CameraPositionDefault.Position);
            }

            window.Title = "dt="+ dt.ToString();

            float moveMult = dt * CameraMoveSpeed;

            if (isSpeedupKeyPressed)
            {
                moveMult = dt * CameraMoveSpeedFast;
            }

            if (isSlowdownKeyPressed)
            {
                moveMult = dt * CameraMoveSpeedSlow;
            }
            
            float x = 0;
            float y = 0;
            float z = 0;

            if (InputTracker.GetKey(Veldrid.Key.D))
                x += 1;
            if (InputTracker.GetKey(Veldrid.Key.A))
                x -= 1;
            if (InputTracker.GetKey(Veldrid.Key.E))
                y += 1;
            if (InputTracker.GetKey(Veldrid.Key.Q))
                y -= 1;
            if (InputTracker.GetKey(Veldrid.Key.W))
                z += 1;
            if (InputTracker.GetKey(Veldrid.Key.S))
                z -= 1;

            MoveCamera(x, y, z, moveMult);
            UpdateOrbitCameraCenter();
            

            if (currentMouseClickR || currentMouseClickM)
            {
                if (!MousePressed)//		First frame of mouse press
                {
                    var mx = InputTracker.MousePosition.X;
                    var my = InputTracker.MousePosition.Y;
                    if (mx >= BoundingRect.Left && mx < BoundingRect.Right && my >= BoundingRect.Top && my < BoundingRect.Bottom)
                    {
                        MousePressed = true;
                        MousePressedPos = InputTracker.MousePosition;
                        Sdl2Native.SDL_ShowCursor(0);
                        Sdl2Native.SDL_SetWindowGrab(window.SdlWindowHandle, true);
                    }
                    
                    if (currentMouseClickM && !lastFrameMouseClickM)//		First frame ClickM is held
                    {
                        shiftWasHeldBeforeClickM = isSpeedupKeyPressed;//		Record shift's state. The camera will orbit/pan based only on the initial state of shift when ClickM is pressed
                    }
                }
                else
                {
                    Vector2 mouseDelta = MousePressedPos - InputTracker.MousePosition;
                    Sdl2Native.SDL_WarpMouseInWindow(window.SdlWindowHandle, (int)MousePressedPos.X, (int)MousePressedPos.Y);

                    //Mouse.SetPosition(game.ClientBounds.X + game.ClientBounds.Width / 2, game.ClientBounds.Y + game.ClientBounds.Height / 2);

                    float camH = mouseDelta.X * 1 * CameraTurnSpeedMouse * 0.0160f;
                    float camV = mouseDelta.Y * -1 * CameraTurnSpeedMouse * 0.0160f;
                    

                    if (currentMouseClickR)//		Look Camera
                    {

                        if (mouseDelta.LengthSquared() == 0)
                        {
                            // Prevents a meme
                            //oldWheel = currentWheel;
                            return true;
                        }

                        if (isMoveLightKeyPressed)
                        {
                            camV = Math.Max(camV, 0);
                            LightRotation.Y += camH;
                            LightRotation.X -= camV;
                        }
                        else
                        {
                            var eul = CameraTransform.EulerRotation;
                            eul.Y -= camH;
                            eul.X += camV;
                            CameraTransform.EulerRotation = eul;
                            UpdateOrbitCameraCenter();
                        }
                    }
                    else if (currentMouseClickM)//	Orbit/Pan camera
                    {
                        if (shiftWasHeldBeforeClickM){//	Pan
                            Vector3 cameraSpacePanDirection = new Vector3(camH, camV, 0);
                            MoveCamera(cameraSpacePanDirection.X, cameraSpacePanDirection.Y, cameraSpacePanDirection.Z, OrbitCamDistance * dt * CameraMoveSpeed * 0.5f);//		CameraMoveSpeed was too fast, cut by half
                            UpdateOrbitCameraCenter();
                        }
                        else//		Orbit
                        {
                            RotateOrbitCamera(camH, camV, Utils.PiOver2);
                        }
                    }
                }


                //CameraTransform.Rotation.Z -= (float)Math.Cos(MathHelper.PiOver2 - CameraTransform.Rotation.Y) * camV;

                //RotateCamera(mouseDelta.Y * -0.01f * (float)moveMult, 0, 0, moveMult);
                //RotateCamera(0, mouseDelta.X * 0.01f * (float)moveMult, 0, moveMult);
            }
            else
            {
                if (MousePressed)
                {
                    Sdl2Native.SDL_WarpMouseInWindow(window.SdlWindowHandle, (int)MousePressedPos.X, (int)MousePressedPos.Y);
                    Sdl2Native.SDL_SetWindowGrab(window.SdlWindowHandle, false);
                    Sdl2Native.SDL_ShowCursor(1);
                    MousePressed = false;
                }

                if (oldMouseClickL)
                {
                    //Mouse.SetPosition((int)oldMouse.X, (int)oldMouse.Y);
                }
                //game.IsMouseVisible = true;
            }



            var eu = CameraTransform.EulerRotation;
            eu.X = Utils.Clamp(CameraTransform.EulerRotation.X, -Utils.PiOver2, Utils.PiOver2);
            CameraTransform.EulerRotation = eu;


            LightRotation.X = Utils.Clamp(LightRotation.X, -Utils.PiOver2, Utils.PiOver2);

            oldClickType = currentClickType;

            oldMouseClickL = currentMouseClickL;
            oldMouseClickR = currentMouseClickR;
            lastFrameMouseClickM = currentMouseClickM;

            oldMouse = mousePos;
            return true;
        }
    }
}
