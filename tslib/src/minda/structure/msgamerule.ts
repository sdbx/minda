export interface MSGameRule {
    /**
     * 지기 위하여 필요한 돌의 갯수
     */
    defeat_lost_stones:number;
    /**
     * 한 턴의 제한시간 (초)
     */
    turn_timeout:number;
    /**
     * 한 게임에 대한 제한시간 (초)
     */
    game_timeout:number;
}