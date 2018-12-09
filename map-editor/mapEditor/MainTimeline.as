package mapEditor {
	import flash.display.*;
	import flash.events.*;
	import flash.events.Event;
	import flash.net.FileReference;

	dynamic
	public class MainTimeline extends MovieClip {
		public var eraser: MovieClip;
		public var blackball: MovieClip;
		public var whiteball: MovieClip;
		public var saveBtn: SimpleButton;
		public var side: int = 5;
		public var distance: Number = 150.55 * 0.4;
		public var map: Array = new Array(side * 2 - 1);
		public var file: FileReference = new FileReference();
		public var selectedTool = 0;

		public function MainTimeline() {
			addFrameScript(0, this.frame1);
		}

		public function frame1() {
			for (var i = 0; i < this.side * 2 - 1; i++) {
				map[i] = new Array(this.side * 2 - 1);
			}
			this.CreateBoard();
			stage.addEventListener(Event.ENTER_FRAME, this.EnterFrameHandler);
			this.eraser.addEventListener(MouseEvent.CLICK, this.eraser_click);
			this.whiteball.addEventListener(MouseEvent.CLICK, this.whiteball_click);
			this.blackball.addEventListener(MouseEvent.CLICK, this.blackball_click);
			this.saveBtn.addEventListener(MouseEvent.CLICK, this.saveBtn_click);
			this.file.addEventListener(Event.COMPLETE, this.fileSaved);
		}

		public function fileSaved(event: Event): void {
			trace("FIle Saved.");
		}

		public function eraser_click(e: Event) {
			this.selectedTool = 0;
		}

		public function blackball_click(e: Event) {
			this.selectedTool = 1;
		}

		public function whiteball_click(e: Event) {
			this.selectedTool = 2;
		}

		public function saveBtn_click(e: Event) {
			var s = this.side - 1;
			var mapStr = "";
			for (var x = -s; x <= s; x++) {
				for (var y = -s; y <= s; y++) {
					if (Math.abs(x + y) <= s) {
						mapStr += map[x + s][y + s].Type;
					}

					mapStr += "@";
				}
				mapStr = mapStr.substr(0, mapStr.length - 1);
				mapStr += "#";
			}
			mapStr = mapStr.substr(0, mapStr.length - 1);
			this.file.save(mapStr, "map.txt");
		}

		public function EnterFrameHandler(e: Event) {
			if (this.selectedTool == 0) {
				this.eraser.gotoAndStop(2);
			} else {
				this.eraser.gotoAndStop(1);
			}
			if (this.selectedTool == 1) {
				this.blackball.gotoAndStop(2);
			} else {
				this.blackball.gotoAndStop(1);
			}
			if (this.selectedTool == 2) {
				this.whiteball.gotoAndStop(2);
			} else {
				this.whiteball.gotoAndStop(1);
			}
		}

		public function CreateBoard() {
			var s = this.side - 1;

			for (var x = -s; x <= s; x++) {
				for (var y = -s; y <= s; y++) {
					if (Math.abs(x + y) <= s) {
						var currentHole = new Hole();
						currentHole.x = x * distance + y * distance / 2 + 350;
						currentHole.y = -y * Math.sqrt(3) * distance / 2 + 300;
						currentHole.scaleX = 0.4;
						currentHole.scaleY = 0.4;
						currentHole.coordX = x + s;
						currentHole.coordY = y + s;
						currentHole.addEventListener(MouseEvent.CLICK, this.clickHex);
						this.addChildAt(currentHole, 0);
						currentHole.Type = 0;
						this.map[x + s][y + s] = currentHole;
					}

				}
			}
		}

		public function clickHex(e: MouseEvent) {
			e.currentTarget.Type = this.selectedTool;
			e.currentTarget.gotoAndStop(this.selectedTool + 1);
		}
	}
}