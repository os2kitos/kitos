appveyor AddMessage "Running eslint"
node_modules\.bin\eslint %ESLINT_DIR%\**\*.js
