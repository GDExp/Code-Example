
namespace game {

    /** New System */
    export class PlayerInput extends ut.ComponentSystem {
        OnUpdate():void {
            this.world.forEach([ut.Entity, ut.Physics2D.Velocity2D, game.InputSystem, game.Player],(entity, currentVelocity, player, animator)=>{
                if((ut.Core2D.Input.getMouseButtonDown(0) || ut.Core2D.Input.touchCount.length > 0) && player.grounded && !player.hit){
                    player.jump = true;
                    this.world.usingComponentData(entity,[ut.Physics2D.SetVelocity2D],(velocity)=>{
                        ChangeVelocity(velocity, player.jumpForce,0);
                    });
                    PlayerInput.ChangeCurrentAnimation(this.world,animator,1);
                }
                if(player.hit && !player.gameOver)
                {
                    player.hit = false;
                    player.gameOver = true;
                    //экран проигрыша игрока = рестар
                    PlayerInput.ChangeCurrentAnimation(this.world,animator,0);
                    ut.EntityGroup.instantiate(this.world,'game.GameOver');
                } 
                if(player.jump && currentVelocity.velocity.y < 0)
                {
                    this.world.usingComponentData(entity,[ut.Physics2D.SetVelocity2D],(velocity)=>{
                        ChangeVelocity(velocity, player.jumpForce,1);
                    });
                    player.jump = false;
                    PlayerInput.ChangeCurrentAnimation(this.world,animator,2);
                }
                
            });

        }

        static ChangeCurrentAnimation(world:ut.World, anim:game.Player, state:number){
            switch(state){
                case(1)://jump
                setEntityEnabled(world,anim.Idle, false);
                setEntityEnabled(world,anim.Jump, true);
                break;
                case(2)://fall
                setEntityEnabled(world,anim.Fall, true);
                setEntityEnabled(world,anim.Jump, false);
                break;
                case(3)://hit
                setEntityEnabled(world,anim.Hit, true);
                this.ChangeCurrentAnimation(world,anim,0);
                
                default://def
                setEntityEnabled(world,anim.Idle, true);
                setEntityEnabled(world,anim.Jump, false);
                setEntityEnabled(world,anim.Fall, false);
                break;
            }
        }
        
    }

    function ChangeVelocity(velocity:ut.Physics2D.SetVelocity2D, force:number, c:number):void{
        let vector: Vector2;
        switch(c){
            case(1):
            vector = new Vector2(0,-1.5 * force);
            break;
            default:
            vector = new Vector2(0, force);
            break;
        }
        velocity.velocity = vector;
    }
    function setEntityEnabled(world: ut.World, entity: ut.Entity, enabled: boolean) {
        let hasDisabledComponent = world.hasComponent(entity, ut.Disabled);
        if (enabled && hasDisabledComponent) {
            world.removeComponent(entity, ut.Disabled);
        }
        else if (!enabled && !hasDisabledComponent) {
            world.addComponent(entity, ut.Disabled);
        }
        world.usingComponentData(entity,[ut.Core2D.Sprite2DSequencePlayer],(sPlayer)=> sPlayer.time = 0);
    }
}
