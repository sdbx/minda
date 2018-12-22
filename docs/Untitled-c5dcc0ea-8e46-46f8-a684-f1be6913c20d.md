# 인증

만약 인증 토큰이 없다면 oauth 인증을 거쳐야 합니다. 로비서버의 POST /auths/를 통해 원하는 서비스(네이버, 디스코드, 구글 등)을 골라 유저가 들어가야 할 서비스 로그인 페이지의 URL과 인증요청 id값을 받습니다.

유저가 서비스 로그인 페이지를 통해 인증을 마쳤을 경우 로비서버의 GET /auths/{인증요청 id}를 통해 토큰을 얻을 수 있습니다.

참고로 현재 개발편의성을 위해 테스트용 토큰(black,white)을 사용할 수 있습니다.