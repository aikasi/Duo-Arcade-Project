using System.Text;
using UnityEngine;

public class HangulAutomata
{
    // 초성, 중성, 종성 테이블
    private static readonly char[] ChoSeong = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
    private static readonly char[] JungSeong = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
    private static readonly char[] JongSeong = { '\0', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };

    // 현재 조합 상태
    private int choIndex = -1;
    private int jungIndex = -1;
    private int jongIndex = -1;

    // 완성된 문자열
    private StringBuilder completedText = new StringBuilder();

    // 현재 입력된 문자를 초기화
    public void Clear()
    {
        choIndex = -1;
        jungIndex = -1;
        jongIndex = -1;
        completedText.Clear();
    }

    // 전체 텍스트 반환 (완료된 것 + 현재 조합 중인 것)
    public string GetFullText()
    {
        return completedText.ToString() + MakeHangul();
    }

    // 문자 입력 처리
    public void InputKey(char input)
    {
        // 1. 자음 입력
        if (IsChoSeong(input))
        {
            int index = GetIndex(ChoSeong, input);
            InputConsonant(index);
        }
        // 2. 모음 입력
        else if (IsJungSeong(input))
        {
            int index = GetIndex(JungSeong, input);
            InputVowel(index);
        }
    }

    // 백스페이스 처리
    public void Backspace()
    {
        // 종성이 있으면 종성 삭제
        if (jongIndex != -1)
        {
            jongIndex = -1;
        }
        // 중성이 있으면 중성 삭제
        else if (jungIndex != -1)
        {
            jungIndex = -1;
        }
        // 초성만 있으면 초성 삭제
        else if (choIndex != -1)
        {
            choIndex = -1;
        }
        // 조합 중인 게 없으면 완료된 텍스트의 마지막 글자 삭제
        else if (completedText.Length > 0)
        {
            char lastChar = completedText[completedText.Length - 1];
            completedText.Length--;

            // 지운 글자를 다시 분해해서 상태 복구 (연속 지우기 위해)
            Decompose(lastChar);
        }
    }

    // 한글 조합 로직 (자음 입력 시)
    private void InputConsonant(int index)
    {
        // 초성이 없으면 초성으로 설정
        if (choIndex == -1)
        {
            choIndex = index;
        }
        // 초성+중성+종성 상태 -> 새 글자 시작 (종성을 초성으로 넘길지 판단 필요하지만 여기선 단순화)
        else if (jongIndex != -1)
        {
            CommitCharacter();
            choIndex = index;
        }
        // 초성+중성 상태 -> 종성으로 입력
        else if (jungIndex != -1)
        {
            // 해당 자음이 종성으로 쓰일 수 있는지 확인
            int jongIdx = GetIndex(JongSeong, ChoSeong[index]); // 초성 배열의 문자를 종성 배열에서 찾음
            if (jongIdx != -1)
            {
                jongIndex = jongIdx;
            }
            else
            {
                CommitCharacter();
                choIndex = index;
            }
        }
        // 초성만 있는 상태 -> 앞 초성 완료하고 새 초성
        else
        {
            CommitCharacter();
            choIndex = index;
        }
    }

    // 한글 조합 로직 (모음 입력 시)
    private void InputVowel(int index)
    {
        // 종성이 있는 상태 -> 종성을 다음 글자 초성으로 넘김 (연음 법칙)
        if (jongIndex != -1)
        {
            int prevJong = jongIndex;
            jongIndex = -1;
            char tempJong = JongSeong[prevJong];

            // 현재 글자 완료
            CommitCharacter();

            // 넘겨받은 종성을 초성으로 세팅
            choIndex = GetIndex(ChoSeong, tempJong);
            jungIndex = index;
        }
        // 중성이 있는 상태 -> 이중 모음 처리 (복잡해서 일단 새 글자로 처리하거나 생략)
        else if (jungIndex != -1)
        {
            CommitCharacter();
            // 초성 없이 모음만 올 경우 (특수 처리 or 무시)
            // 여기선 완성된 글자 뒤에 모음만 붙는 건 허용 안 하거나 독립 모음으로 처리
            completedText.Append(JungSeong[index]);
        }
        // 초성이 있는 상태 -> 합체
        else if (choIndex != -1)
        {
            jungIndex = index;
        }
        // 아무것도 없으면 모음 단독 출력
        else
        {
            completedText.Append(JungSeong[index]);
        }
    }

    // 현재 조합 중인 글자를 완성 문자열로 넘김
    private void CommitCharacter()
    {
        completedText.Append(MakeHangul());
        choIndex = -1;
        jungIndex = -1;
        jongIndex = -1;
    }

    // 현재 인덱스로 한글 한 글자 생성
    private string MakeHangul()
    {
        // 조합 중인 게 없으면 빈 문자열
        if (choIndex == -1 && jungIndex == -1 && jongIndex == -1) return "";

        // 초성만 있음
        if (choIndex != -1 && jungIndex == -1) return ChoSeong[choIndex].ToString();

        // 중성만 있음 (특수 케이스)
        if (choIndex == -1 && jungIndex != -1) return JungSeong[jungIndex].ToString();

        // 초성 + 중성 (+ 종성)
        int tempJong = (jongIndex != -1) ? jongIndex : 0;
        // 유니코드 공식: 0xAC00 + (초성x21x28) + (중성x28) + 종성
        int unicode = 0xAC00 + (choIndex * 21 * 28) + (jungIndex * 28) + tempJong;
        return ((char)unicode).ToString();
    }

    // 문자 분해 (지우기 위해)
    private void Decompose(char c)
    {
        if (c >= 0xAC00 && c <= 0xD7A3)
        {
            int baseCode = c - 0xAC00;
            jongIndex = baseCode % 28;
            jungIndex = ((baseCode - jongIndex) / 28) % 21;
            choIndex = ((baseCode - jongIndex) / 28) / 21;

            if (jongIndex == 0) jongIndex = -1; // 종성 없음
        }
        else
        {
            // 한글 아님 (자음 단독 등) -> 다시 초성이나 중성으로 복구 시도
            // 단순화를 위해 그냥 비움
            choIndex = -1; jungIndex = -1; jongIndex = -1;
        }
    }

    // 헬퍼 함수들
    private int GetIndex(char[] array, char target) { for (int i = 0; i < array.Length; i++) if (array[i] == target) return i; return -1; }
    private bool IsChoSeong(char c) { return GetIndex(ChoSeong, c) != -1; }
    private bool IsJungSeong(char c) { return GetIndex(JungSeong, c) != -1; }
}