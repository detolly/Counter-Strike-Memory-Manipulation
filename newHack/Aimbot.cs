using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static newHack.Form1;
using static hazedumper.netvars;
using static hazedumper.signatures;
using System.Windows.Forms;

namespace newHack
{
    public class Aimbot
    {
        public struct Vector3
        {
            public float x, y, z;
            public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
            public float Length()
            {
                return (float)Math.Abs(Math.Sqrt(System.Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)));
            }

            public static float Dot(Vector3 vector1, Vector3 vector2)
            {
                return (vector1.x * vector2.x) + (vector1.y * vector2.y) + (vector1.z * vector2.z);
            }

            public static Vector3 operator /(Vector3 left, Vector3 right)
            {
                return new Vector3(left.x / right.x, left.y / right.y, left.z / right.z);
            }

            public static Vector3 operator +(Vector3 left, Vector3 right)
            {
                return new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
            }

            public static Vector3 operator -(Vector3 left, Vector3 right)
            {
                return new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
            }

            public static Vector3 operator /(Vector3 left, int right)
            {
                return new Vector3(left.x / right, left.y / right, left.z / right);
            }

            internal Angles ToAngles()
            {
                return new Angles(this.x, this.y);
            }
        }

        public struct Angles
        {
            public static Angles operator +(Angles left, Angles right)
            {
                return new Angles(left.x + right.x, left.y + right.y);
            }
            public static Angles operator -(Angles left, Angles right)
            {
                return new Angles(left.x - right.x, left.y - right.y);
            }
            public static Angles operator *(Angles left, float right)
            {
                return new Angles(left.x * right, left.y * right);
            }
            public float Length()
            {
                return (float)Math.Abs(System.Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));
            }

            public float x, y;

            public Angles(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public Vector3 ToVector()
            {
                return new Vector3(this.x, this.y, 0);
            }
        }

        static int GetBaseEntity(int PlayerNumber, int bClient, VAMemory vam)
        {
            int BaseEntity = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (PlayerNumber * Offsets.oEntityLoopDistance)));
            if (BaseEntity != 0)
            {
                return BaseEntity;
            }
            else
            {
                return 0;
            }
        }

        public static Vector3 GetBonePosition(int boneIndex, int PlayerNumber, int bClient, VAMemory vam)
        {
            int BaseEntity = GetBaseEntity(PlayerNumber, bClient, vam);
            if (BaseEntity != 0)
            {
                int BoneMatrix = vam.ReadInt32((IntPtr)(BaseEntity + Offsets.dwBoneMatrix));
                int hp = vam.ReadInt32((IntPtr)(BaseEntity + Offsets.oHealth));
                if (BoneMatrix != 0 && hp > 0)
                {
                    float x = vam.ReadFloat((IntPtr)(BoneMatrix + 0x30 * boneIndex + 0x0C));
                    float y = vam.ReadFloat((IntPtr)(BoneMatrix + 0x30 * boneIndex + 0x1C));
                    float z = vam.ReadFloat((IntPtr)(BoneMatrix + 0x30 * boneIndex + 0x2C));
                    return new Vector3(x, y, z);
                }
                return new Vector3(99999f, 99999f, 99999f);
            }
            return new Vector3(99999f, 99999f, 99999f);
        }

        public static Angles GetPunch(int playerIndex, VAMemory vam, int bClient)
        {
            int start = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (playerIndex * Offsets.oEntityLoopDistance)));
            if (start != 0)
            {
                float x = 0;
                float y = 0;
                x = vam.ReadFloat((IntPtr)(start + Offsets.dwPunchOffset));
                y = vam.ReadFloat((IntPtr)(start + Offsets.dwPunchOffset + 4));
                return new Angles(x, y);
            }
            else
            {
                return new Angles(999f, 999f);
            }
        }

        public static Angles getCurrentAngles(VAMemory vam, int engine)
        {
            float x = vam.ReadFloat((IntPtr)(vam.ReadInt32((IntPtr)(engine + Offsets.oClientState)) + Offsets.oViewAngles));
            float y = vam.ReadFloat((IntPtr)(vam.ReadInt32((IntPtr)(engine + Offsets.oClientState)) + Offsets.oViewAngles + 4));
            return new Angles(x, y);
        }

        public static Vector3 getLocalPlayerPosition(VAMemory vam, int bClient)
        {
            int playerIndex = vam.ReadInt32((IntPtr)(vam.ReadInt32((IntPtr)(bClient + Offsets.oLocalPlayer)) + 0x64));
            int start = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (playerIndex - 1) * Offsets.oEntityLoopDistance));
            float x = vam.ReadFloat((IntPtr)(start + Offsets.vecOrigin));
            float y = vam.ReadFloat((IntPtr)(start + Offsets.vecOrigin + 4));
            float z = vam.ReadFloat((IntPtr)(start + Offsets.vecOrigin + 8));
            return new Vector3(x, y, z);
        }

        public static Vector3 getLocalPlayerEyePosition(VAMemory vam, int bClient)
        {
            int playerIndex = vam.ReadInt32((IntPtr)(vam.ReadInt32((IntPtr)(bClient + Offsets.oLocalPlayer)) + 0x64));
            int start = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (playerIndex - 1) * Offsets.oEntityLoopDistance));
            float x = vam.ReadFloat((IntPtr)(start + Offsets.vecOrigin));
            float y = vam.ReadFloat((IntPtr)(start + Offsets.vecOrigin + 4));
            float z = vam.ReadFloat((IntPtr)(start + Offsets.vecOrigin + 8));

            x += vam.ReadFloat((IntPtr)(start + m_vecViewOffset));
            y += vam.ReadFloat((IntPtr)(start + m_vecViewOffset + 4));
            z += vam.ReadFloat((IntPtr)(start + m_vecViewOffset + 8));
            return new Vector3(x, y, z);
        }

        public static Vector3 getEntityEyePosition(VAMemory vam, int bClient, int index)
        {
            int start = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + index * Offsets.oEntityLoopDistance));
            float x = vam.ReadFloat((IntPtr)(start + Offsets.vecOrigin));
            float y = vam.ReadFloat((IntPtr)(start + Offsets.vecOrigin + 4));
            float z = vam.ReadFloat((IntPtr)(start + Offsets.vecOrigin + 8));

            x += vam.ReadFloat((IntPtr)(start + m_vecViewOffset));
            y += vam.ReadFloat((IntPtr)(start + m_vecViewOffset + 4));
            z += vam.ReadFloat((IntPtr)(start + m_vecViewOffset + 8));
            return new Vector3(x, y, z);
        }

        public static Angles calcang(Vector3 src, Vector3 dst, VAMemory vam, int bClient)
        {
            //Angles MyPunch = GetPunch(vam, bClient);
            double[] delta = { (src.x - dst.x), (src.y - dst.y), (src.z - dst.z) };
            double hyp = Math.Sqrt(delta[0] * delta[0] + delta[1] * delta[1]);
            float x = (float)(Math.Asin(delta[2] / hyp) * 57.295779513082f);
            float y = (float)(Math.Atan(delta[1] / delta[0]) * 57.295779513082f);
            if (delta[0] >= 0.0)
            {
                y += 180.0f;
            }
            return new Angles(x, y);
        }

        public static bool isAlive(int index, VAMemory vam, int bClient)
        {
            int baseentity = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (index) * Offsets.oEntityLoopDistance));
            if (vam.ReadInt32((IntPtr)(baseentity + Offsets.oHealth)) > 0 && vam.ReadInt32((IntPtr)(baseentity + Offsets.oHealth)) <= 100)
            {
                return true;
            }
            return false;
        }

        public static bool isEntityByIdOnMyTeam(int localIndex, int Entityindex, VAMemory vam, int bClient)
        {
            int baseentity = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (Entityindex) * Offsets.oEntityLoopDistance));
            int localentity = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (localIndex) * Offsets.oEntityLoopDistance));
            if (baseentity != 0 && localentity != 0)
            {
                int entityTeam = vam.ReadInt32((IntPtr)(baseentity + Offsets.oTeam));
                if (entityTeam != 2 && entityTeam != 3) return true;
                int myTeam = vam.ReadInt32((IntPtr)(localentity + Offsets.oTeam));
                if (entityTeam == myTeam)
                    return true;
                return false;
            }
            return true;
        }

        public bool calcang2(int playerIndex, Vector3 src, Vector3 dst, VAMemory vam, int bClient, int bEngine, float currentFov, int currentThing, float smoothing, Form1 form)
        {
            Angles MyPunch = GetPunch(playerIndex, vam, bClient);
            double[] delta = { (src.x - dst.x), (src.y - dst.y), (src.z - (dst.z)) };
            double hyp = Math.Sqrt(delta[0] * delta[0] + delta[1] * delta[1]);
            float y = (float)(Math.Atan(delta[1] / delta[0]) * 180/Math.PI) - MyPunch.y*2f;
            float x = (float)(Math.Atan2(delta[2], hyp) * 180/Math.PI) - MyPunch.x*2f;
            Angles setAngles = new Angles(x, y);
            float fov = currentFov;
            if (delta[0] >= 0.0)
            {
                setAngles.y += 180.0f;
            }
            float hisFov = getFov(setAngles, src, dst, vam, bEngine, playerIndex, bClient);
            Console.WriteLine(hisFov);
            if (hisFov < fov || form.rageBot)
            {
                Point aimAt = new Point();
                if (form.legitBot)
                {
                    WorldToScreen(dst, ref aimAt, vam, getMatrixFloats2(form.csgo, bClient), form.t);
                    previous = MyPunch;
                    if (aimAt.x > 0 && aimAt.x < form.t.Right && aimAt.y > 0 && aimAt.y < form.t.Bottom)
                    {
                        moveMouse((int)(aimAt.x), (int)(aimAt.y), form.smoothing, form.t);
                        return true;
                    }
                }
                else if (form.rageBot && (hisFov < fov || !form.careAboutFovInRage))
                {
                    setAngles = NormaliseViewAngle(setAngles.ToVector(), getCurrentAngles(vam,bEngine).ToVector()).ToAngles();
                    vam.WriteFloat((IntPtr)(vam.ReadInt32((IntPtr)(bEngine + Offsets.oClientState)) + Offsets.oViewAngles), setAngles.x);
                    vam.WriteFloat((IntPtr)(vam.ReadInt32((IntPtr)(bEngine + Offsets.oClientState)) + Offsets.oViewAngles + 4), setAngles.y);
                    return true;
                }
            }
            return false;
        }

        public void RCS(int playerIndex, VAMemory vam, int bClient, int bEngine, RECT t, float smoothing)
        {
            Angles MyPunch = GetPunch(playerIndex, vam, bClient) * 2f;
            float thisX = AngletoScreenX(MyPunch.x, previous.x, t);
            float thisY = AngletoScreenY(MyPunch.y, previous.y, t);
            if (thisX > 0 || thisY > 0)
                moveMouse((int)(t.Right / 2 + thisX), (int)(t.Bottom / 2 + thisY), smoothing, t);
            previous = MyPunch;
        }

        Angles previous;

        private static float getFov(Angles aimAngle, Vector3 src, Vector3 dst, VAMemory vam, int bEngine, int index, int bClient)
        {
            Angles viewangles = getCurrentAngles(vam, bEngine) + GetPunch(index, vam, bClient) * 2f;
            float distance = Get3dDistance(src, dst);
            float pitch = (float)Math.Sin((float)((viewangles.x - aimAngle.x) * Math.PI / 180f)) * distance;
            float yaw = (float)Math.Sin((float)((viewangles.y - aimAngle.y) * Math.PI / 180f)) * distance;
            float fov = (float)Math.Sqrt(pitch * pitch + yaw * yaw);
            return fov;
            //Vector3 forward = AngleVectors(NormaliseViewAngle(getCurrentAngles(vam, bEngine).ToVector()));
            //Vector3 deltaVector = dst - src;
            //Normalize(deltaVector, out deltaVector);
            //float dotp = Vector3.Dot(deltaVector, forward);
            //float hisFov = (float)(Math.Acos(dotp) * (180f / Math.PI));
            //return hisFov;
        }

        [DllImport("User32.dll")]
        public static extern void mouse_event(int dwFlags, int x, int y, IntPtr dwData, UIntPtr dwExtraInfo);

        static void moveMouse(int x, int y, float smooth, RECT t)
        {
            x = (int)(t.Right / 2 + (x - t.Right / 2) * smooth);
            y = (int)(t.Bottom / 2 + (y - t.Bottom / 2) * smooth);
            mouse_event(0x1 | 0x8000, x - t.Right / 2, y - t.Bottom / 2, IntPtr.Zero, UIntPtr.Zero);
        }

        static void click()
        {
            mouse_event(0x2, 0, 0, IntPtr.Zero, UIntPtr.Zero);
            System.Threading.Thread.Sleep(1);
            mouse_event(0x4, 0, 0, IntPtr.Zero, UIntPtr.Zero);
        }

        static Vector3 NormaliseViewAngle(Vector3 angle, Vector3 previous)
        {
            while (angle.y <= -180) angle.y += 360;
            while (angle.y > 180) angle.y -= 360;
            while (angle.x <= -180) angle.x += 360;
            while (angle.x > 180) angle.x -= 360;

            if (angle.x > 89.0f) angle.x = 89f;
            if (angle.x < -89.0f) angle.x = -89f;
            if (angle.y < -180f) angle.y = -179.999f;
            if (angle.y > 180f) angle.y = 179.999f;
            if (angle.x != angle.x)
                angle.x = previous.x;
            if (angle.y != angle.y)
                angle.y = previous.y;

            angle.z = 0;
            return angle;
        }

        static void Normalize(Vector3 vIn, out Vector3 vOut)
        {
            float flLen = vIn.Length();
            if (flLen == 0)
            {
                vOut = new Vector3(0, 0, 1);
                return;
            }
            flLen = 1 / flLen;
            vOut = new Vector3(vIn.x * flLen, vIn.y * flLen, vIn.z * flLen);
        }


        public static float map(float input, float min1, float max1, float min2, float max2)
        {
            var start = (input - min1) / (max1 - min1);
            return ((max2 - min2) * start / 100) + min2;
        }

        public static Vector3 getPlayerVectorByIndex(VAMemory vam, int bClient, int index)
        {
            float x = 0f, y = 0f, z = 0f;
            //get enemy coords;

            int PtrToPIC = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (index) * Offsets.oEntityLoopDistance));
            if (PtrToPIC != 0)
            {
                x = vam.ReadFloat((IntPtr)(PtrToPIC + Offsets.vecOrigin));
                y = vam.ReadFloat((IntPtr)(PtrToPIC + Offsets.vecOrigin + 4));
                z = vam.ReadFloat((IntPtr)(PtrToPIC + Offsets.vecOrigin + 8));
                if (x == 0 || y == 0 || z == 0)
                {
                    x = 99999999f;
                    y = 99999999f;
                    z = 99999999f;
                }
                return new Vector3(x, y, z);
            }
            else
            {
                return new Vector3(999999f, 999999f, 999999f);
            }
        }

        public static float Get3dDistance(Vector3 from, Vector3 to)
        {
            return (float)Math.Sqrt(
                (Math.Pow((to.x - from.x), 2)) +
                (Math.Pow((to.y - from.y), 2)) +
                (Math.Pow((to.z - from.z), 2)));
        }

        public class Point
        {
            public float x, y;
            public Point(float x, float y)
            {
                this.x = x; this.y = y;
            }
            public Point() { }
        }

        public static float[][] getMatrixFloats2(Memory mem, int bClient)
        {
            int pointer = bClient + Offsets.oViewMatrix;
            byte[] data = mem.ReadMatrix(pointer, 4, 4);
            float[][] fA = new float[4][];
            for (int y = 0; y < 4; y++)
            {
                fA[y] = new float[4];
                for (int x = 0; x < 4; x++)
                {
                    fA[y][x] = BitConverter.ToSingle(data, sizeof(float) * ((y * 4) + x));
                }
            }
            return fA;
        }

        public static bool WorldToScreen(Vector3 from, ref Point point, VAMemory vam, float[][] m_vMatrix, RECT rectangle)
        {

            float w = 0.0f;

            Point to = new Point();

            to.x = m_vMatrix[0][0] * from.x + m_vMatrix[0][1] * from.y + m_vMatrix[0][2] * from.z + m_vMatrix[0][3];
            to.y = m_vMatrix[1][0] * from.x + m_vMatrix[1][1] * from.y + m_vMatrix[1][2] * from.z + m_vMatrix[1][3];
            w = m_vMatrix[3][0] * from.x + m_vMatrix[3][1] * from.y + m_vMatrix[3][2] * from.z + m_vMatrix[3][3];

            if (w < 0.01f)
                return false;

            Vector3 temp;
            temp.x = to.x / w;
            temp.y = to.y / w;

            int width = rectangle.Right - rectangle.Left;
            int height = rectangle.Bottom - rectangle.Top;

            float x = width / 2;
            float y = height / 2;

            x += (float)(0.5 * temp.x * width + 0.5);
            y -= (float)(0.5 * temp.y * height + 0.5);

            to.x = x + rectangle.Left;
            to.y = y + rectangle.Top;

            point = to;
            return true;
        }

        static Vector3 AngleVectors(Vector3 angles)
        {
            float sp, sy, cp, cy;

            SinCos(angles.x / (180 / Math.PI), out sy, out cy);

            SinCos(angles.y / (180 / Math.PI), out sp, out cp);

            Vector3 forward = new Vector3();
            forward.x = cp * cy;
            forward.y = cp * sy;
            forward.z = -sp;
            return forward;
        }

        private static void SinCos(double v, out float sy, out float cy)
        {
            sy = (float)Math.Sin(v);
            cy = (float)Math.Cos(v);
        }




        float AngletoScreenX(float angle, float previous, RECT screen)
        {
            //D = W tanB / 2Tan(A / 2)
            //X
            int width = 0;
            int height = 0;
            width = screen.Right;
            height = screen.Bottom;
            int A = (int)(74 * (Math.PI / 180));
            float theta = (float)((angle - previous) * (Math.PI / 180));
            float X = (float)((height * Math.Tan(theta)) / (2 * Math.Tan(37 * (Math.PI / 180))));
            //X /= 1000;
            //Y
            // 4:3 90, 16:9 106, 16:10 100
            /*
            int B = 100;
            double Y = (W* tan(angle.y*(PI / 180))) / (2 * tan(B / 2));
            Vector screen;
            screen.y = Y;
            screen.x = X;
            return screen;
            */
            return X;
        }
        float AngletoScreenY(float angle, float previous, RECT screen)
        {
            //D = W tanB / 2Tan(A / 2)
            //X
            int width = 0;
            int height = 0;
            width = screen.Right;
            height = screen.Bottom;
            if (width <= 0 || height <= 0) { return 0; }
            int B = 100;
            if (width / height == 4 / 3)
            {
                B = 90;
            }
            else if (width / height == 16 / 9)
            {
                B = 106;
            }
            else
            {
                B = 100;

            }

            float A = (float)((B / 2) * (Math.PI / 180));
            float theta = (float)((angle - previous) * (Math.PI / 180));
            float Y = (float)((height * Math.Tan(theta)) / (2 * Math.Tan(A)));
            //X /= 1000;
            //Y
            // 4:3 90, 16:9 106, 16:10 100
            /*
            int B = 100;
            double Y = (W* tan(angle.y*(PI / 180))) / (2 * tan(B / 2));
            Vector screen;
            screen.y = Y;
            screen.x = X;
            return screen;
            */
            return Y;
        }
    }
}