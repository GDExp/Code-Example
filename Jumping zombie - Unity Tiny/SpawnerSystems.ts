
namespace game {

    /** New System */
    export class SpawnerSystems extends ut.ComponentSystem {
        static spawner: ut.Entity = null;
        OnUpdate():void {
            if(game.SpawnerSystems.spawner  == null){
                this.world.forEach([ut.Entity, game.Spawner],(entity)=>{
                    game.SpawnerSystems.spawner = new ut.Entity(entity.index, entity.version);
                    SpawnerSystems.BlockSpawn(this.world);
                    
                });
            }
        }

        static BlockSpawn(world:ut.World) {
            world.usingComponentData(this.spawner,[game.Spawner],(spw)=>{
                ut.EntityGroup.instantiate(world,'game.BlocksGroup')[0];
                if(spw.plusSpeed < 3) spw.plusSpeed += 0.25;
                spw.isPaused = true;
            });
        }
    }

    
}
