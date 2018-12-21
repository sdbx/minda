# 기본적인 사용법

서버는 크게 로비서버와 게임서버로 나뉩니다. 로비서버의 주소는 미리 알아야합니다. 게임서버의 주소는 로비서버가 알려줄 것이므로 미리 알 필요 없습니다.

## 로비서버

게임서버는 http를 통해 접근할 수 있습니다.

위 문서에 주소들이 세부문서로 정리되어 있습니다. 세부문서의 이름을 보고 주소를 어떻게 접근할지 간략하게 알 수 있습니다. 이름의 형식이 "(http명령어) (주소)" 되어있기 때문입니다. 접근하기 위해 인증이 필요한 경우 #가 http명령어와 주소 사이에 들어있는데 인증 토큰을 Authorization 헤더에 넣으면 됩니다. 인증 토큰을 얻는 과정은 인증 문서에 정리되어 있습니다.

예를 들어 방 리스트를 얻기 위해 GET # /rooms/를 아래와 같은 코드로 접근할 수 있습니다.

    const url = "로비서버주소";
    const token = "토큰";
    
    fetch(url+"/rooms/", {
    	method: "get",
    	headers: {
        "Authorization": token,
    		"Content-Type": "application/json"
      }
    }).then((res) => {
    	if (!res.ok) {
    		throw Error(res.status);
    	}
      return res;
    }).then((res) => {
    	console.log(res.json());
    });

세부문서안의 입력과 출력을 보고 어떤 바디를 보낼지와 어떤 내용이 올 지 알 수 있습니다. 참고로 둘 다 json 형태입니다. 가끔 주소안에 중괄호로 감싸진 무언가가 있는 경우가 있는데 이는 주소로 입력받는 무언가입니다. 예를 들어 GET /auths/{authid} 의 경우 /auths/asdf, /auths/gorani, /auths/1234 등으로 접근될 수 있습니다.

## 게임서버

게임서버는 소켓을 통해 접근할 수 있습니다. 클라이언트는 게임서버에게 무언가 시키기 위해 명령을 전송합니다. 게임서버는 클라이언트에게 무슨일이 생기면 이벤트를 전송합니다. 

위 문서에 명령과 이벤트의 종류가 세부문서로 정리되어 있습니다. 명령과 이벤트 모두 json 형식이며 type 필드를 기준으로 종류가 구분됩니다. 세부문서의 이름은 type필드의 값입니다. 

예를 들어 채팅을 주고 받기 위해 아래와 같은 코드를 사용할 수 있습니다.

    const juso = "게임서버주소";
    const invite = "초대키";
    
    let net = require("net");
    let client = new net.Socket();
    
    client.connect(5353, juso, function() {
    	client.write(JSON.stringify({
    		type: "connect",
    		invite: invite
    	}));
    	client.write(JSON.stringify({
    		type: "chat",
    		content: "끼 로 좋 아"
    	}));
    });
    
    client.on("data", function(data) {
    	let obj = JSON.parse(data);
    	if (obj.type === "chated") {
    		console.log(obj.user + ":" + obj.content);	
    	}
    });