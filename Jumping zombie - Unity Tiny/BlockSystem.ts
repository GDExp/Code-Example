//не всегда работает!
namespace game {
/*
    export class BlockSystemFilter extends ut.EntityFilter {
        entity: ut.Entity;
        position: ut.Core2D.TransformLocalPosition;
        sprite: ut.Core2D.Sprite2DRenderer;
        system: game.MoveBlock;
        spawner: game.Spawner;
    }
    
    export class BlockSystem extends ut.ComponentBehaviour {

        data: BlockSystemFilter;
        

        // ComponentBehaviour lifecycle events
        // uncomment any method you need
        
        // this method is called for each entity matching the BlockSystemFilter signature, once when enabled
        OnEntityEnable():void { 
            if(this.data.system.move)
            {
                let n_speed = this.data.system.blockSpeed + getRandom(1, 2);
                let color = new ut.Core2D.Color(1, getRandom(1,255) / 255, getRandom(1,255) /getRandom(1,255), 1);

                this.data.system.blockSpeed = n_speed;
                this.data.sprite.color = color;
    
                let randomX = getRandom(-1,1);
                let n_pos = new Vector3((randomX>0)?6:-6, 1 + 0.05, 0);
                this.data.position.position = n_pos;
           
            }
        }
        
        // this method is called for each entity matching the BlockSystemFilter signature, every frame it's enabled
        OnEntityUpdate():void { 
            if(!this.data.system.move) return;

            let localPosition = this.data.position.position;
            
            if(Math.abs(localPosition.x) > 0.05  && this.data.system.move){
                if(this.data.position.position.x > 0) {
                    localPosition.x -= this.data.system.blockSpeed * this.scheduler.deltaTime();
                }
                else{
                    localPosition.x +=this.data.system.blockSpeed * this.scheduler.deltaTime();
                }
            }
            else{
                if(this.data.system.move)
                console.log("End");

                this.data.system.move = false;

            }
        }

        // this method is called for each entity matching the BlockSystemFilter signature, once when disabled
        //OnEntityDisable():void { }.

    }

    function getRandom(min:number, max:number) {
        return Math.random() * (max - min + 1) + min;
    }//*/
}
