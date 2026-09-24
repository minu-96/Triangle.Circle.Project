# 삼각원 · STOVE 상품 디자인 묶음

따뜻한 크림 배경, 다섯 가지 색 도형, 민트 선택 칸으로 기존 게임과 같은 인상을 유지합니다. 정사각형 작은 아이콘에도 도형 퍼즐이라는 장르가 알아보이도록 도형을 격자 안에 크게 배치했습니다.

## 검토 이미지

- [그래픽 전체 모음](preview-contact-sheet.png)
- [STOVE 등록용 이미지 모아보기](stove-upload/preview-contact-sheet.png)
- [STOVE 등록 항목별 입력 안내](stove-upload/registration-fields.ko.md)
- [STOVE 등록용 파일 폴더](stove-upload/)
- [화면 디자인 시안 5장](contact-sheet.png)
- [메인 키아트 16:9](assets/store-key-art-16x9-1920x1080.png)
- [상품 페이지 미리보기](product-page-preview.html)
- [앱 아이콘 512px](assets/app-icon-512.png)
- [상세 상품 페이지 카피](store-page.ko.md)
- [전체 이미지 목록과 픽셀 크기](asset-manifest.json)
- [STOVE 업로드 파일 목록과 픽셀 크기](stove-upload/manifest.json)
- [브랜드 가이드](brand-guide.md)
- [출시 빌드 캡처 목록](CAPTURE-CHECKLIST.md)
- [SVG 로고 원본](source/samgakwon-wordmark.svg)
- [SVG 앱 아이콘 원본](source/app-icon.svg) · [투명 SVG 워드마크](source/wordmark-transparent.svg) · [PNG 워드마크](assets/wordmark-transparent-1500x360.png)
- [키아트 생성 스크립트](source/generate_store_art.py) · [화면 시안 스크립트](source/generate_screen_designs.py) · [로고 스크립트](source/generate_wordmark.py) · [STOVE 업로드 모아보기 생성기](source/generate_upload_preview.py) · [STOVE 슬롯 이미지 생성기](source/generate_stove_uploads.py)

## 포함 파일

`assets/`에는 512px/1024px 앱 아이콘, 정사각형 카드, 16:9 키아트, 와이드 배너, 커뮤니티 배너, 소셜 카드, 그리고 **앞서 만든 HTML 플레이어블 화면을 출처 그대로 보존한 참조 이미지**가 있습니다.
`screen-designs/`에는 Unity UI 방향을 보여주는 세로 화면 5장(메인, 난이도, 스테이지, 플레이, 클리어)이 있습니다. [미리보기](contact-sheet.png). 이 5장은 UI 구성 시안이며 게임 실행 화면 캡처라고 표시하지 않습니다.

## 원본 이미지 다시 만들기

Unity 프로젝트에 포함된 Nanum Gothic 폰트를 사용하며 Python과 Pillow가 필요합니다. 아래 순서로 키아트·배너·아이콘, 화면 시안, 투명 워드마크를 생성합니다.

```sh
python3 Release/Stove/source/generate_store_art.py
python3 Release/Stove/source/generate_screen_designs.py
python3 Release/Stove/source/generate_wordmark.py
```

Python 의존성: [requirements.txt](source/requirements.txt).

## Studio에서 적용

`stove-upload/`에는 사용자가 알려준 Studio 화면의 픽셀 규격에 맞춘 파일이 있습니다. [등록 안내](stove-upload/registration-fields.ko.md)에서 각 입력란과 파일 경로를 확인하세요. 정적 배경은 기본 타입에 맞춰 준비했고, 한국어 기준 언어용 전시 그래픽 5장도 포함했습니다. `manifest.json`에서 실제 이미지 픽셀 크기와 파일 형식을 확인할 수 있습니다.

전시 그래픽은 게임 규칙·모드를 설명하는 제작 이미지입니다. 실제 플레이 캡처로 오인되지 않도록 안내했으며, 출시 전에는 Unity 최종 빌드 화면을 촬영해 실제 캡처 이미지로 교체하는 것을 권합니다. 가격, 출시일, 이용등급, OS와 PC 사양, 링크 등 확정되지 않은 정보는 지어내지 않았습니다. 상품 유형·계약·빌드 요구사항은 STOVE Studio의 현재 개발사 안내를 따르세요.

[스토브 공식 출시 절차 가이드](https://developers.onstove.com/en/docs/stove/start/onboarding) · [STOVE Studio PC 등록 준비 안내](https://studio-docs.onstove.com/en/pc/GettingStarted/requisition.html)

한국어 폰트는 프로젝트의 Nanum Gothic과 OFL 라이선스를 참조합니다. SVG는 `Nanum Gothic` 사용을 전제로 하는 편집 원본이며, PNG에는 해당 폰트로 글자를 렌더링했습니다.
