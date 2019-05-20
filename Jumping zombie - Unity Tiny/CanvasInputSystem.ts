
namespace game {

    /** New System */
    export class CanvasInputSystem extends ut.ComponentSystem {
        private static mainScene: string = "game.MainScene";
        private static scores: string = "game.GameUI";
        private static blocks:string = "game.BlocksGroup";
        OnUpdate():void {
            this.world.forEach([game.ButtonInfo, ut.UIControls.MouseInteraction],(button, mInteraction) =>{
                if(mInteraction.clicked){
                    newGame(this.world, button.menuState);
                }
            })

        }
    }

    function newGame(world:ut.World, state: number){

        switch(state){
            case(1)://close main menu
            ut.EntityGroup.destroyAll(world, 'game.MainMenu');
            newGame(world,0);
            break;
            case(2):// restart
            ut.EntityGroup.destroyAll(world,'game.BlocksGroup')
            ut.EntityGroup.destroyAll(world,'game.GameUI');
            ut.EntityGroup.destroyAll(world,'game.GameOver');
            ut.EntityGroup.destroyAll(world,'game.MainScene')
            SpawnerSystems.spawner = null;
            ScoreSystem.scoreEntity = null;
            newGame(world,0);
            break;
            default://new game
            ut.EntityGroup.instantiate(world, 'game.MainScene');
            ut.EntityGroup.instantiate(world, 'game.GameUI');
            break;
        }
        
        
    }
}
