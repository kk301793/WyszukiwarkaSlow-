.model flat
.data
.code

public AsmStart

AsmStart proc
   	; w modelu flat rejestry CS,DS i ES maja ta sama wartosc 
	; a DS:[SI] i ES:[DI] bedziemy porownywac

	mov		edx, [esp+16]			; pobierz czwarty argument - ile znaków w linii do przeszukania
	mov		esi, [esp+12]			; pobierz trzeci argument - wsk. na linie 
	mov		edi, [esp+8]			; pobierz drugi argument - wsk. na s³owo ktore szukamy  
	mov		ecx, [esp+4]			; pobierz pierwszy argument - wielkosc slowa szukanego

	mov		ebx, esi			; pobierz trzeci argument - wsk. na linie 
   	xor		eax, eax			; wyzeruj 
	add		edx, esi			; do testowania wskaŸnika koñca linii

PETLA:
	mov		esi, ebx		; przywroc poprawna wartosc rejestru esi (indeks w stringu, w ktorym trzeba szukac)
	mov		edi, [esp+8]		; przywroc poprawna wartosc rejestru edi (poczatek ciagu znakow, ktorego szukac w stringu)
	inc		ebx			; zwieksz wskaznik, ktory bedzie wrzucony do esi w kolejnej iteracji

	mov		ecx, [esp+4]		; ustaw limit dzialania 'repz cmpsb' na wielkosc slowa szukanego
	cld
	repe		cmpsb			; porownuj bajty spod esi, z bajtami spod edi
								; ciagi równe je¿eli CX=0	
	test		ecx, ecx		; jesli zero, to wszystkie bajty spod edi pasowaly do bajtow spod esi - znaczy, ze znalezlismy ciag
	jz		JEST			; jesli zero, to znaleziono
	jmp		BRAK
JEST:
	inc		eax
BRAK:
	cmp 		ebx, edx		; czy osiagnieto koniec szukania w linii
	jb		PETLA			;jump if Below 

	ret
AsmStart endp

end