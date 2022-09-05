using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// First combat algorithm by Luis
/// </summary>
public class Battle : MonoBehaviour
{

	[SerializeField] int HPPlayer; //vida del personaje jugable
	[SerializeField] int HPEnemy; //vida del enemigo
	int maxHPPlayer;
	int playerEvasion; //la evasion del personaje
	int enemyEvasion; //la evasion del enemigo
	int blindDebufDuration; //los turnos que dura el debuf de cegado
	int blindDebufTimer; //el timer del debufo de cegado para que al acabarse se quite el cegado
	int blindDebuf; //lo que afecta el debufo de cegado
	int defenceDebuf; //lo que afecta el debufo de defensa
	int randomNumber; //un placeholder para los numeros aleatorios
	int maceDamage; //daño de la maza
	int healingAmount;//cuanto se cura
	int actualHealing;
	int enemyMeleeDamage;//daño del enemigo a melee
	int enemyRangeDamage;//daño del enemigo a distancia

	float animTimer; //timer provisional para simular el tiempo de las animaciones

	bool blind; //para ver si se esta cegado o no
	bool defence; //para ver si se tiene defensa o no
	bool playerStage; //para ver si es el turno dle jugador o no
	bool playerChoosing; //para ver si el jugador esta eleigiendo una opcion
	bool enemyChoosing; //para evitar que cada frame el enemigo machaque una opcion
	bool dead;//para ver si alguno ha muerto


	void Start()
	{
		Reference();//donde se refencia todas las variables

		Debuglogs(1, " ");
	}

	void Reference()
	{
		HPPlayer = 100;
		HPEnemy = 100;
		maxHPPlayer = HPPlayer;
		playerEvasion = 90;
		enemyEvasion = 90;
		blindDebufDuration = 2;
		blindDebufTimer = blindDebufDuration;
		blindDebuf = 1;
		defenceDebuf = 1;
		maceDamage = 15;
		healingAmount = 20;
		actualHealing = 0;
		enemyMeleeDamage = 10;
		enemyRangeDamage = 10;

		animTimer = 3;

		blind = false;
		defence = false;
		playerStage = true;
		playerChoosing = true;
		enemyChoosing = false;

		dead = false;
	}

	void Update()
	{
		//esto permite que en cuanto uno muera el juego no continue
		if (!dead)
		{
			Stages();
		}
		else
		{
			return;
		}
	}

	void Stages()
	{
		if (playerStage)
		{
			PlayerOptions(); //las opciones del jugador
		}
		else if (enemyChoosing && !playerStage )
		{
			enemyChoosing = false ;
			EnemyAttack();
		}
		else
        {
			return;
        }
	}

	void PlayerOptions()
	{
		Mace();//al elegir la opcion de Maza
		BlindLight();//al elegir la opcion de luz cegadora
		Healing(); //al elegir la opcion de curacion
		Defence(); //al elegir defensa
		Nothing();//hacer nada, esto es mas que nada para probar la animacion de muerte del personaje
	}

	void Mace()
	{
		if (Input.GetKeyDown(KeyCode.Keypad1) && playerChoosing)
		{
			//al pulsar el boton 1 se pone como falso el choosing para que no se pueda pulsar mas botones.
			playerChoosing = false;
			//se genera un numero para ver si se acierta comparando el resultado con la evasion del enemigo
			randomNumber = Random.Range(1, 100);

			Debuglogs(2,"Maza");

			Debuglogs(5, "Has");

			if (randomNumber < enemyEvasion)
			{
				StartCoroutine(MaceHit());//si acierta
			}
			else
			{
				StartCoroutine(MaceMiss());//si falla
			}

		}
	}

	IEnumerator MaceHit()
	{
		
		//animacion y audios
		
		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion
		Debuglogs(3, "acertado");
		//efectos post aniamcion y animacion enemiga
		HPEnemy -= maceDamage;
		Debuglogs(4, " ");
		//en caso de acabar con el enemigo se comprueba
		if (HPEnemy<=0)
		{
			StartCoroutine(EnemyDead());//se va a la pantalla de muerte
		}
		else
		{
			playerStage = false;//se cambia al turno enemigo
			enemyChoosing = true;
			Debuglogs(6, "del enemigo");
		}
	}

	IEnumerator MaceMiss()
	{

		//animacion y audios

		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion
		Debuglogs(3, "fallado");
		//efectos post aniamcion y animacion enemiga

		playerStage = false;//se cambia al turno enemigo
		enemyChoosing = true;
		Debuglogs(6, "del enemigo");
	}

	void BlindLight()
	{
		if (Input.GetKeyDown(KeyCode.Keypad2) && playerChoosing)
		{
			//al pulsar el boton 2 se pone como falso el choosing para que no se pueda pulsar mas botones.
			playerChoosing = false;
			//se genera un numero para ver si se acierta comparando el resultado con la evasion del enemigo
			randomNumber = Random.Range(1, 100);

			Debuglogs(2, "Luz Cegadora");

			Debuglogs(5, "Has");

			if (randomNumber < enemyEvasion && !blind)
			{
				StartCoroutine(BlindLightHit());//si acierta
			}
			else
			{
				StartCoroutine(BlindLightMiss());//si falla
			}

		}
	}

	IEnumerator BlindLightHit()
	{

		//animacion y audios

		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion
		Debuglogs(3, "acertado");
		//efectos post aniamcion y animacion enemiga
		blind = true;
		Debuglogs(7, "cegar al enemigo");
		playerStage = false;//se cambia al turno enemigo
		enemyChoosing = true;
		Debuglogs(6, "del enemigo");

	}

	IEnumerator BlindLightMiss()
	{

		//animacion y audios

		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion
		Debuglogs(3, "fallado");
		//efectos post aniamcion y animacion enemiga

		playerStage = false;//se cambia al turno enemigo
		enemyChoosing = true;
		Debuglogs(6, "del enemigo");

	}

	void Healing()
	{
		if (Input.GetKeyDown(KeyCode.Keypad3) && playerChoosing)
		{
			//al pulsar el boton 3 se pone como falso el choosing para que no se pueda pulsar mas botones.
			playerChoosing = false;

			Debuglogs(2, "Curacion");

			StartCoroutine(StartHealing());
			
		}
	}

	IEnumerator StartHealing()
	{

		//animacion y audios

		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion

		//efectos post aniamcion y animacion enemiga
		HPPlayer += healingAmount;
		if (HPPlayer > maxHPPlayer)
        {
			actualHealing = HPPlayer- maxHPPlayer;

			HPPlayer = maxHPPlayer;
        }
		else
        {
			actualHealing = healingAmount;

		}
		Debuglogs(7, "curarte "+ actualHealing);

		Debuglogs(8, " ");

		playerStage = false;//se cambia al turno enemigo
		enemyChoosing = true;
		Debuglogs(6, "del enemigo");

	}

	void Defence()
	{
		if (Input.GetKeyDown(KeyCode.Keypad4) && playerChoosing)
		{
			//al pulsar el boton 4 se pone como falso el choosing para que no se pueda pulsar mas botones.
			playerChoosing = false;

			Debuglogs(2, "Defensa");

			StartCoroutine(StartDefence());

		}
	}

	IEnumerator StartDefence()
	{

		//animacion y audios

		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion

		//efectos post aniamcion y animacion enemiga
		defence = true;
		Debuglogs(7, "defenderte");
		playerStage = false;//se cambia al turno enemigo
		enemyChoosing = true;
		Debuglogs(6, "del enemigo");

	}

	void Nothing()
	{
		if (Input.GetKeyDown(KeyCode.Keypad5) && playerChoosing)
		{
			//al pulsar el boton 5 se pone como falso el choosing para que no se pueda pulsar mas botones.
			playerChoosing = false;

			Debuglogs(2, "Nada");

			playerStage = false;
			enemyChoosing = true;
			Debuglogs(6, "del enemigo");

		}
	}

	IEnumerator EnemyDead()
	{
		dead = true;
		//aniamcion de muerte y pantallazo de vistoria
		Debuglogs(9, " ");
		yield return new WaitForSeconds(animTimer);

	}

	void EnemyAttack()
	{
		randomNumber = Random.Range(1, 3);
		//se escoge al azar si el enemigo atacara a distancia o a melee
		Debuglogs(5, "Como ataque ha");
		if (randomNumber == 1)
		{
			MeleeAttack();
		}
		else if (randomNumber == 2)
		{
			RangeAttack();
		}
	}

	void MeleeAttack()
	{
		//se comprueba si el enemigo esta cegado, en caso afirmativo se aplicara el debufo y despues de x rondas se autosedactivara
		if (blind)
		{
			blindDebuf = 2;
			blindDebufTimer -= 1;
			if (blindDebufTimer <= 0)
			{
				blind = false;
				blindDebuf = 1;
				blindDebufTimer = blindDebufDuration;
				Debuglogs(11, "esta cegado el enemigo");
			}
		}
		Debuglogs(12, "hecho un ataque a melee");
		//se comprueba si el player esta en defensa
		if (defence)
		{
			defenceDebuf = 2;
		}
		else
		{
			defenceDebuf = 1;
		}

		randomNumber = Random.Range(1, 100);
		//se genera un numero aletaorio para evr si acierta al jugador

		Debuglogs(5, "Ha");
		if (randomNumber < (playerEvasion / blindDebuf))
		{
			StartCoroutine(MeleeAttackHit());
		}
		else
		{
			StartCoroutine(MeleeAttackMiss());
		}
		
	}

	IEnumerator MeleeAttackHit()
	{
		//animacion y audios

		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion
		HPPlayer -= (enemyMeleeDamage / defenceDebuf);
		//efectos post aniamcion y animacion enemiga
		Debuglogs(12, "acertado");
		Debuglogs(13, " ");
		Debuglogs(8, " ");
		if (defence)
		{
			defence = false;
			Debuglogs(11, "te estas defendiendo");
		}
		//en caso de acabar con el player se comprueba
		if (HPPlayer<= 0)
		{
			StartCoroutine(PlayerDead());//se va a la pantalla de muerte
		}
		else
		{
			//se retorna al turno del jugador y se le deja elegir
			playerStage = true;
			playerChoosing = true;
			Debuglogs(6, "del jugador");
			Debuglogs(1, " ");
		}

	}

	IEnumerator MeleeAttackMiss()
	{
		//animacion y audios

		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion
		Debuglogs(12, "fallado");
		//efectos post aniamcion y animacion enemiga
		if (defence)
		{
			defence = false;
			Debuglogs(11, "te estas defendiendo");
		}

		//se retorna al turno del jugador y se le deja elegir
		playerStage = true;
		playerChoosing = true;
		Debuglogs(6, "del jugador");
		Debuglogs(1, " ");
	}

	void RangeAttack()
	{
		//se comprueba si el enemigo esta cegado, en caso afirmativo se aplicara el debufo y despues de x rondas se autosedactivara
		if (blind)
		{
			blindDebuf = 2;
			blindDebufTimer -= 1;
			if (blindDebufTimer <= 0)
			{
				blind = false;
				blindDebuf = 1;
				blindDebufTimer = blindDebufDuration;
				Debuglogs(11, "esta cegado el enemigo");
			}
		}

		Debuglogs(12, "hecho un ataque de rango");

		//se comprueba si el player esta en defensa
		if (defence)
		{
			defenceDebuf = enemyRangeDamage;
		}
		else
		{
			defenceDebuf = 0;
		}

		randomNumber = Random.Range(1, 100);
		//se genera un numero aletaorio para evr si acierta al jugador
		Debuglogs(12, "acertado");

		Debuglogs(5, "Ha");
		if (randomNumber < (playerEvasion / blindDebuf))
		{
			StartCoroutine(RangeAttackHit());
		}
		else
		{
			StartCoroutine(RangeAttackMiss());
		}

	}

	IEnumerator RangeAttackHit()
	{
		//animacion y audios

		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion
		HPPlayer -= (enemyRangeDamage - defenceDebuf);
		//efectos post aniamcion y animacion enemiga
		Debuglogs(12, "acertado");
		Debuglogs(14, " ");
		Debuglogs(8, " ");

		if (defence)
		{
			defence = false;
			Debuglogs(11, "te estas defendiendo");
		}
		//en caso de acabar con el player se comprueba
		if (HPPlayer <= 0)
		{
			StartCoroutine(PlayerDead());//se va a la pantalla de muerte
		}
		else
		{
			//se retorna al turno del jugador y se le deja elegir
			playerStage = true;
			playerChoosing = true;
			Debuglogs(6, "del jugador");
			Debuglogs(1, " ");
		}

	}

	IEnumerator RangeAttackMiss()
	{
		//animacion y audios

		yield return new WaitForSeconds(animTimer); //tiempo que dura la animacion
		Debuglogs(12, "fallado");
		//efectos post aniamcion y animacion enemiga

		if (defence)
		{
			defence = false;
			Debuglogs(11, "te estas defendiendo");
		}

		//se retorna al turno del jugador y se le deja elegir
		playerStage = true;
		playerChoosing = true;
		Debuglogs(6, "del jugador");
		Debuglogs(1, " ");
	}

	IEnumerator PlayerDead()
	{
		dead = true;
		//aniamcion de muerte y pantallazo de derrota
		Debuglogs(10, " ");
		yield return new WaitForSeconds(animTimer);

	}

	void Debuglogs(int p, string j)
	{
		if (p == 1)
        {
			Debug.Log("Use los botones numericos para elegir la siguiente opcion: 1.Maza/ 2.Luz Cegadora / 3.Curacion / 4.Defensa / 5.Nada");
		}
		else if (p == 2)
        {
			Debug.Log("Has utilizado "+j);
		}
		else if (p == 3)
        {
			Debug.Log("Has " +j);
		}
		else if (p == 4)
        {
			Debug.Log("El enemigo recibe " + maceDamage + " de daño");
			Debug.Log("El enemigo tiene "+ HPEnemy+ " de vida");
		}
		else if (p == 5)
        {
			Debug.Log(j+ " sacado un " +randomNumber);

		}
		else if (p == 6)
        {
			Debug.Log("Turno "+ j);
		}
		else if (p == 7)
        {
			Debug.Log("Has logrado " + j);
		}
		else if (p == 8)
        {
			Debug.Log("Tienes " + HPPlayer+ " de vida");
		}
		else if (p == 9)
        {
			Debug.Log("El enemigo ha muerto");
			Debug.Log("Victoria");
			Debug.Log("Fin de la Partida");
		}
		else if (p == 10)
		{
			Debug.Log("El jugador ha muerto");
			Debug.Log("Derrota");
			Debug.Log("Fin de la Partida");
		}
		else if (p == 11)
        {
			Debug.Log("Ya no "+j);
		}
		else if (p == 12)
        {
			Debug.Log("El enemigo ha " + j);
		}
		else if (p == 13)
        {
			Debug.Log("Has recibido "+ (enemyMeleeDamage / defenceDebuf)+ "de daño");
		}
		else if (p == 14)
		{
			Debug.Log("Has recibido " + (enemyMeleeDamage - defenceDebuf) + "de daño");
		}
		else
        {
            return;
        }
        
	}

}
