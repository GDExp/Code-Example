
namespace game {

    /** New System */
    export class BlockMovement extends ut.ComponentSystem {
        
        OnUpdate():void {
            this.world.forEach([ut.Core2D.Sprite2DRenderer, ut.Core2D.TransformLocalPosition, game.MoveBlock], (sprite, transform, block ) =>{
                
                if(block.setup && block.move){
                    let speed_change;
                    this.world.forEach([game.Spawner],(objSpawner)=>{
                        speed_change = objSpawner.plusSpeed;

                    });
                    let n_speed = block.blockSpeed + getRandom(speed_change * 0.75, speed_change);
                    let color = new ut.Core2D.Color(getRandom(0.1,1),getRandom(0.1,1),getRandom(0.1,1),1)

                    block.blockSpeed = n_speed;
                    sprite.color = color;
    
                    let randomX = getRandom(-1,1);
                    let n_pos = new Vector3((randomX>0)?8:-8, -7 , 0);
                    transform.position = n_pos;
                    block.setup = false;
                }
                if(block.blockSpeed == 0 || !block.move) return;

                let localPosition = transform.position;
            
                if(Math.abs(localPosition.x) > 0.1){
                    if(localPosition.x > 0) {
                        localPosition.x -= block.blockSpeed * this.scheduler.deltaTime();
                    }
                    else{
                        localPosition.x +=block.blockSpeed * this.scheduler.deltaTime();
                    }
                    transform.position = localPosition;
                }
                else{
                    if(block.move)
                    block.blockSpeed= 0;
                }

            });
                        

        }
    }

    function getRandom(min:any, max:any){
        return Math.random() * (max - min + 1) + min;
    }
}
