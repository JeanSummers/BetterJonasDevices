using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BetterJonasDevices
{
    internal class BlockEntityDischargedBaseReturnTeleporter : BlockEntity
    {
        BlockEntityAnimationUtil AnimUtil => GetBehavior<BEBehaviorAnimatable>().animUtil;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (api.World.Side == EnumAppSide.Client)
            {
                float rotY = Block.Shape.rotateY;
                AnimUtil.InitializeAnimator("basereturnteleporter-discharged", null, null, new Vec3f(0, rotY, 0));
                var meta = new AnimationMetaData() { Animation = "deploy", Code = "deploy", BlendMode = EnumAnimationBlendMode.Average };
                AnimUtil.StartAnimation(meta);
            }
        }
    }
}
