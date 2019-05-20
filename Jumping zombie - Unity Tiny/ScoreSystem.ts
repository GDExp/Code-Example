
namespace game {

    /** New System */
    export class ScoreSystem extends ut.ComponentSystem {
        
        static scoreEntity: ut.Entity = null;
        OnUpdate():void {
            if(game.ScoreSystem.scoreEntity == null){
                this.world.forEach([ut.Entity, game.Score], (entity, score)=>{
                    game.ScoreSystem.scoreEntity = new ut.Entity(entity.index, entity.version);
                    ScoreSystem.AddScore(this.world,0);
                });
            }

        }

        static AddScore(world: ut.World, state:number){

            world.usingComponentData(this.scoreEntity,[ut.Text.Text2DRenderer, game.Score], (text, score)=>{
                switch(state){
                    case(1)://add
                    score.Scores ++;
                    break;
                    default://start setup
                    score.Scores = 0;
                    break;
                }
                text.text = score.Scores.toString();

            });
        }
    }
}
