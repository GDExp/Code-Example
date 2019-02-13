using UnityEngine;

//класс юнитов игрока и оновные взаимодействия
public class EarthUnit : UnitMechanics
{
    private Transform point_transform;
    private PointInfo current_point;
    
    private float aid_timer;

    //движение к точке
    public void PlayerChosePoint(PointInfo player_point, Transform point)
    {
        if(current_point != null)
        {
            player_controller.PointIsBooked(null, point_transform);
            current_point.SetUnitInPoint(null);
        }
        point_transform = point;
        current_point = player_point;
        current_point.SetUnitInPoint(this);
        player_controller.PointIsBooked(this, point_transform);
        game_controller.GetUnitWay(this, ref unit_way, current_point.point_position);
    }
    //смерть юнита игрока
    public override void UnitDead()
    {
        aid_timer = 0f;
        player_controller.CheckSelectedUnit(this);
        player_controller.PointIsBooked(null, point_transform);
        current_point.SetUnitInPoint(null);
        current_point = null;
        base.UnitDead();
    }

    public void SetAidReloadTime()
    {
        aid_timer = Time.time + 30f;
    }

    public float GetAidReloadTime()
    {
        return aid_timer;
    }
    //to late--->>>
    public float GetDeltaHealth()
    {
        float delta = max_health / current_health;
        return delta;
    }
}
