# RU
# Программа для скачивания репозитория, а так же быстрого обновления мода [Punisher](https://github.com/dyeness/trader_punisher).

## Как пользоваться:
1. Качаем актуальный релиз и распаковываем
2. Открываем папку и находим файл `Settings.json` и редактируем его как нам удобно:
   ```
    {
    "RepositoryUrl": "https://github.com/dyeness/trader_punisher",      ! Ссылка на репозиторий !
    "TargetPath": "C:\\Games\\EscapeFromTarkov\\user"                   ! Путь к папке с модами, обязательно указать папку USER !
    }
    ```
3. Указываем путь в пункте `"TargetPath":` в таком формате `C:\\Folders\\EscapeFromTarkov\\user`, обязательно ставим `\\`, вместо `\`.
   - Где `C:` - раздел диска;
   - Где `Folders` - папки, которые ведут к корневой папки с модами.
4. После настройки установщика запускаем его, консоль покажет название репозитория, репозиторий и релизы (работает плохо)
5. Программа автоматически скачает репозиторий и заменит файлы мода на нужные
6. Готово

> [!WARNING]
> После всех операций нужно удалить временную папку репозитрия `TempRepo` внутри папки с программой, если она не удалилась сама



# ENG
# Program for downloading the repository, as well as quick updates to the [Punisher] mod(https://github.com/dyeness/trader_punisher).

## How to use:
1. Download the current release and unzip it
2. Open the folder and find the `Settings.json` file and edit it as we like:
    ```
    {
    "RepositoryUrl": "https://github.com/dyeness/trader_punisher",      ! Repository link !
    "TargetPath": "C:\\Games\\EscapeFromTarkov\\user"                   ! Path to the mods folder, be sure to specify the USER folder !
    }
    ```
3. Specify the path in the item `“TargetPath”:` in this format `C:\Folders\\EscapeFromTarkov\\\\user`, be sure to put `\\` instead of `\`.
   - Where `C:` is a disk partition;
   - Where `Folders` - folders that lead to the root folder with mods.
4. After configuring the installer run it, the console will show the name of the repository, repository and releases (works poorly)
5. The program will automatically download the repository and replace the mod files with the necessary ones
6. Done

> [!WARNING]
> After all operations you need to delete the temporary repository folder `TempRepo` inside the folder with the program, if it is not deleted by itself
