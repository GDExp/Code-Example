using UnityEngine;
//класс юнитов ИИ
public class MarsUnit : UnitMechanics
{
    private bool iRush;

    //тест
    private void Update()
    {
        if (iRush && unit_way.Count == 0)
        {
            game_controller.MarsUnitGo(true);
            player_controller.RefreshUnitUI(unit_ui, 1f, 9);
            game_controller.ReturnObjectInPool(rb_unit, unit_type);
            ResetUnitValue();
            iRush = false;
            iMove = false;
        }
    }

    public void ChoseWay(byte index_way)
    {
        game_controller.GetUnitWay(transform.localPosition, ref unit_way, index_way);
        game_controller.MarsUnitGo(false);
        iRush = true;
    }
    //смерть юнита ИИ
    public override void UnitDead()
    {
        game_controller.MarsUnitGo(true);
        player_controller.KillingMarsUnit();//+++
        base.UnitDead();
    }
}
