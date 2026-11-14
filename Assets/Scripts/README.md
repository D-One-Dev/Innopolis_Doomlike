# Doom-like: скрипты геймплея

Набор скриптов для 3D FPS: игрок, оружие, подбираемые предметы и враги на NavMesh.
HUD нет — здоровье, оружие и боезапас пишутся в консоль.

## Структура

```
Assets/Scripts/
  Core/
    Combat/      урон, здоровье, интерфейс IDamageable
    Player/      ввод, движение, здоровье игрока, сбор предметов, цель для врагов
    Logging/     консольный лог геймплея
  Weapons/       конфиги оружия, инвентарь, hitscan/projectile-стрельба
  Pickups/       оружие, патроны, аптечки
  Enemies/       мозг врага на NavMeshAgent и три типа атаки
  Installers/    Zenject-инсталлер
```

## 1. Настройка сцены

1. Создайте на сцене пустой объект `SceneContext`.
2. Добавьте на него компонент `SceneContext` (Zenject) и перетащите `GameplayInstaller` в список Installers.
3. В `GameplayInstaller` задайте:
   - `Starting Weapon` — стартовое оружие (можно оставить пустым);
   - `Starting Ammo` — стартовый боезапас;
   - `Player Max Health` — максимальное здоровье игрока;
   - `Log Gameplay Events` — вывод состояния в консоль.

Все объекты, лежащие в сцене, Zenject внедряет автоматически. Врагов и снаряды,
создаваемых в рантайме, нужно инстанцировать через `DiContainer.InstantiatePrefab`,
иначе `EnemyBrain` не получит `PlayerTargetService`.

## 2. Игрок

Иерархия:

```
Player (Rigidbody, CapsuleCollider, PlayerMotor, PlayerDamageable,
        PlayerPickupCollector, PlayerTargetRegistration,
        PlayerWeaponController)
  Camera (Camera)
    Muzzle (пустой объект на уровне дула)
```

- Телo игрока — на слое, который не пересекается с маской попадания оружия.
- На игроке должны быть `Rigidbody` и `Collider` (лучше `CapsuleCollider`).
  `Rigidbody` — не кинематический (`Is Kinematic` выключен).
  `PlayerMotor` сам настраивает `Rigidbody` (`useGravity`, `freezeRotation`,
  интерполяцию и continuous collision detection).
- `PlayerMotor`: назначьте `Camera Transform` и `Ground Mask` (слои земли).
  Если камера не назначена, возьмётся `Camera.main`.
- `PlayerWeaponController`: назначьте `View Camera`, `Muzzle` и `Hit Mask`
  (слои, по которым стреляет игрок).
- Камера должна быть дочерней у `Player`, иначе вращение по вертикали не сработает.

Управление (из готового `InputSystem_Actions`):

| Действие | Клавиша |
| --- | --- |
| Движение | WASD |
| Обзор | мышь |
| Огонь | ЛКМ |
| Прыжок | Space |
| Бег | Left Shift |
| Предыдущее / следующее оружие | 1 / 2 |

## 3. Оружие

Создайте ассеты через `Create > Doomlike`:

1. `Ammo Definition` — тип патронов (`Display Name`, `Max Amount`).
   Например: Bullets (200), Shells (50), Rockets (25), Cells (300).
2. `Weapon Config` — параметры оружия:
   - `Fire Mode`: `Hitscan` или `Projectile`;
   - `Ammo`: ссылка на `Ammo Definition`;
   - `Ammo Per Shot`, `Pickup Ammo`;
   - `Damage`, `Pellets Per Shot`, `Spread Degrees`, `Range`;
   - `Fire Interval`, `Automatic`;
   - для projectile-оружия: `Projectile Prefab`, `Projectile Speed`.

Для projectile-оружия создайте префаб снаряда:

```
Projectile (Projectile, сфера/капсула в качестве визуала)
```

`Projectile` сам двигается через SphereCast, наносит урон и уничтожается при попадании.
`Hit Mask` на снаряде настройте так, чтобы он не цеплял игрока и предметы.

Hitscan-попадания визуализируются через `HitscanImpactService`: в точке попадания
появляется декаль-отверстие (живёт 6 секунд с плавным затуханием) и короткая жёлтая
вспышка. Текстуры назначаются на `GameplayInstaller`:

- `Impact Mark Texture` — обязательна, PNG с прозрачностью (круг/отверстие);
- `Impact Flash Texture` — опциональна, при пустом поле используется `Impact Mark Texture`.

Эффекты рендерятся через `SpriteRenderer`, поэтому форма определяется альфа-каналом
текстуры. Отладочные линии (`Draw Hitscan Debug`) остаются только для Editor.

Примеры конфигов:

- **Pistol**: Hitscan, 10 урона, интервал 0.25, одиночный.
- **Shotgun**: Hitscan, 8 дробин по 6 урона, разброс 4°, интервал 0.8.
- **Rocket Launcher**: Projectile, 60 урона, скорость 18, интервал 1.0.

## 4. Подбираемые предметы

На объект добавьте коллайдер и один из компонентов:

- `WeaponPickup` — ссылка на `Weapon Config`, количество патронов при подборе;
- `AmmoPickup` — ссылка на `Ammo Definition`, количество;
- `HealthPickup` — количество восстанавливаемого здоровья.

`PickupBase` сам делает коллайдер триггером и добавляет kinematic `Rigidbody`.
Если предмет не нужен игроку (нет места в инвентаре, полное здоровье), он не исчезает.

## 5. Враги

1. Запеките NavMesh: `Window > AI > Navigation` (пакет AI Navigation уже установлен).
2. Соберите префаб:

```
Enemy (NavMeshAgent, HealthBehaviour, EnemyBrain, один из компонентов атаки)
  Eye (опционально, точка обзора)
```

3. Компоненты атаки:
   - `MeleeAttack` — урон вблизи без снаряда;
   - `ProjectileAttack` — стреляет префабом снаряда;
   - `HitscanAttack` — мгновенный выстрел с проверкой линии видимости.

`EnemyBrain` сам ищет игрока, преследует его и атакует, когда тот в радиусе атаки.
Настройки: `Aggro Range`, `Lose Target Range`, `Repath Interval`, `Turn Speed`,
`Death Animation Duration`. После смерти отключаются коллайдеры и агент, модель
за `Death Animation Duration` секунд сжимается до нуля, после чего объект удаляется.
При нулевой длительности объект удаляется сразу.

Типы врагов задаются комбинацией параметров и компонента атаки:
- быстрый ближний — `MeleeAttack`, мало здоровья, большая скорость;
- стрелок — `ProjectileAttack`, держит дистанцию;
- снайпер — `HitscanAttack`, большой радиус атаки и маленький кулдаун.

## 6. Консольный вывод

`GameplayLogger` пишет в консоль:

```
[Health] 100/100
[Weapon] Shotgun (12 ammo)
[Ammo] Shells: 12/50
[Player] Dead
```

Отключить лог можно флагом `Log Gameplay Events` в `GameplayInstaller`.
