
namespace game {

    /** New System */
    export class PlayerCollisionSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            this.world.forEach([ut.Physics2D.ColliderContacts, game.InputSystem, game.Player], (contact, player, anim)=> {
                let contacts = contact.contacts;

                if(contacts.length > 0){
                    player.jump = false;
                    if(this.world.hasComponent(contacts[0], game.MoveBlock)){
                        let comp = this.world.getComponentData(contacts[0], game.MoveBlock);
                        if(comp.move){
                            comp.move = false;
                            this.world.setComponentData(contacts[0],comp);
                            this.world.forEach([ut.Core2D.TransformLocalPosition, game.Scroller],(transform)=>{
                                let currentPosition = transform.position;
                                currentPosition.y -=1.05;
                                transform.position = currentPosition;
                            });
                            ScoreSystem.AddScore(this.world,1);
                            SpawnerSystems.BlockSpawn(this.world);
                            PlayerInput.ChangeCurrentAnimation(this.world, anim,0);
                        }
                    }

                    if(this.world.hasComponent(contacts[0], game.Ground) && !player.grounded){
                        player.grounded = true;
                    }
                    if(this.world.hasComponent(contacts[0],game.Block) && !player.hit){
                        player.hit = true;
                    }
                    if(contacts.length > 1){
                        if(this.world.hasComponent(contacts[1],game.Block) && !player.hit){
                            player.hit = true;
                        }
                    }
                }else{
                    player.grounded = false;
                    player.jump = true;
                }
                if(player.hit){;
                    PlayerInput.ChangeCurrentAnimation(this.world,anim,3);
                    this.world.forEach([ut.Entity, game.Spawner],(entity ,spawner)=>{
                        spawner.isPaused = true;
                        this.world.setComponentData(entity ,spawner);
                    });
                    this.world.forEach([ game.MoveBlock], (blocks)=>{
                        blocks.move = false;
                    });
                }
            });

        }
    }
}
