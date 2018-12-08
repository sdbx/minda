use game::{Cord, Board, Stone, Move, Player};

#[test]
fn board_test1() {
    use self::Stone::*;
    let mut before = Board::new(5);
    before.set(Cord(0,0,0), Black);
    before.set(Cord(0,-1,1), Black);
    before.set(Cord(0,-2,2), Black);
    before.set(Cord(0,-3,3), White);

    let mut after = Board::new(5);
    after.set(Cord(0,-1,1), Black);
    after.set(Cord(0,-2,2), Black);
    after.set(Cord(0,-3,3), Black);
    after.set(Cord(0,-4,4), White);

    let m = Move{
        from: Cord(0,0,0),
        to: Cord(0,-2,2),
        dir: Cord(0,-1,1),
        player: Player::Black
    };

    before.push(m).unwrap();
    assert_eq!(before.raw(), after.raw());
}

#[test]
fn board_test2() {
    use self::Stone::*;
    let mut before = Board::new(5);
    before.set(Cord(0,0,0), Black);
    before.set(Cord(0,-1,1), Black);
    before.set(Cord(0,-2,2), White);
    before.set(Cord(0,-3,3), White);

    let mut after = Board::new(5);
    after.set(Cord(0,0,0), Black);
    after.set(Cord(0,-1,1), Black);
    after.set(Cord(0,-2,2), White);
    after.set(Cord(0,-3,3), White);

    let m = Move{
        from: Cord(0,0,0),
        to: Cord(0,-2,2),
        dir: Cord(0,-1,1),
        player: Player::Black
    };

    assert!(before.push(m).is_err());
    assert_eq!(before.raw(), after.raw());
}

#[test]
fn board_test3() {
    use self::Stone::*;
    let mut before = Board::new(5);
    before.set(Cord(0,0,0), Black);
    before.set(Cord(0,-1,1), Black);
    before.set(Cord(0,-2,2), Black);
    before.set(Cord(0,-3,3), White);

    let mut after = Board::new(5);
    after.set(Cord(-1,1,0), Black);
    after.set(Cord(-1,0,1), Black);
    after.set(Cord(-1,-1,2), Black);
    after.set(Cord(0,-3,3), White);

    let m = Move{
        from: Cord(0,0,0),
        to: Cord(0,-2,2),
        dir: Cord(-1,1,0),
        player: Player::Black
    };

    before.push(m).unwrap();
    assert_eq!(before.raw(), after.raw());
}

#[test]
fn board_test4() {
    use self::Stone::*;
    let mut before = Board::new(5);
    before.set(Cord(0,0,0), Black);
    before.set(Cord(0,-1,1), Black);
    before.set(Cord(0,-2,2), Black);
    before.set(Cord(0,-3,3), White);

    let mut after = Board::new(5);
    after.set(Cord(-1,1,0), Black);
    after.set(Cord(0,-1,1), Black);
    after.set(Cord(0,-2,2), Black);
    after.set(Cord(0,-3,3), White);

    let m = Move{
        from: Cord(0,0,0),
        to: Cord(0,0,0),
        dir: Cord(-1,1,0),
        player: Player::Black
    };

    before.push(m).unwrap();
    assert_eq!(before.raw(), after.raw());
}

#[test]
fn board_test5() {
    use self::Stone::*;
    let mut before = Board::new(5);
    before.set(Cord(0,0,0), Black);
    before.set(Cord(0,-1,1), Black);

    let mut after = Board::new(5);
    after.set(Cord(0,0,0), Black);
    after.set(Cord(0,-1,1), Black);

    let m = Move{
        from: Cord(0,0,0),
        to: Cord(0,0,0),
        dir: Cord(0,-1,1),
        player: Player::Black
    };

    assert!(before.push(m).is_err());
    assert_eq!(before.raw(), after.raw());
}
