// JavaScript Runtime for C# Bytecode Execution with Phaser
class CSharpBytecodeRuntime {
    constructor() {
        this.stack = [];
        this.variables = new Map();
        this.objects = new Map();
        this.classes = new Map();
        this.methodAddresses = new Map();
        this.instructions = [];
        this.pc = 0; // Program counter
        this.callStack = [];
        this.phaserGame = null;
        this.currentScene = null;
        this.gameObjects = new Map();
        this.nextObjectId = 1;
    }

    loadBytecode(bytecodeJson) {
        const bytecode = JSON.parse(bytecodeJson);
        
        // Load class definitions
        for (const [className, classDef] of Object.entries(bytecode.Classes)) {
            this.classes.set(className, classDef);
        }
        
        // Load instructions
        this.instructions = bytecode.Instructions;
        
        // Load method addresses
        for (const [methodName, address] of Object.entries(bytecode.MethodAddresses)) {
            this.methodAddresses.set(methodName, address);
        }
        
        console.log(`Loaded ${this.classes.size} classes, ${this.instructions.length} instructions`);
        return bytecode.EntryPoint;
    }

    initializePhaser(config) {
        // Create Phaser game configuration
        const phaserConfig = {
            type: Phaser.AUTO,
            width: config.Width || 320,
            height: config.Height || 640,
            scale: {
                mode: Phaser.Scale.EXPAND,
                autoCenter: Phaser.Scale.CENTER_HORIZONTALLY
            },
            parent: config.Parent || 'game-container',
            backgroundColor: config.BackgroundColor || '#3d4042',
            scene: {
                preload: () => this.phaserPreload(),
                create: () => this.phaserCreate(),
                update: (time, delta) => this.phaserUpdate(time, delta)
            }
        };

        this.phaserGame = new Phaser.Game(phaserConfig);
        return this.phaserGame;
    }

    phaserPreload() {
        // Create simple colored rectangles as placeholders for assets
        const scene = this.phaserGame.scene.scenes[0];
        
        // Create a simple background texture
        scene.add.graphics()
            .fillStyle(0x2c3e50)
            .fillRect(0, 0, 320, 640);
            
        // We'll create the player sprite programmatically in the create method
    }

    phaserCreate() {
        this.currentScene = this.phaserGame.scene.scenes[0];
        
        // Create cursor keys
        this.cursors = this.currentScene.input.keyboard.createCursorKeys();
        
        // Create the player directly since the bytecode execution is simulated
        console.log('Creating player sprite...');
        const player = this.createPlayer(160, 320, 'pixel-man'); // Center of 320x640 screen
        const objId = this.nextObjectId++;
        this.gameObjects.set(objId, player);
        this.variables.set('Player', objId);
        
        console.log('Player created at position:', player.x, player.y);
    }

    phaserUpdate(time, delta) {
        // Update all game objects
        for (const [id, obj] of this.gameObjects) {
            if (obj.update) {
                obj.update(time, delta);
            }
        }
    }

    execute(entryPoint) {
        console.log(`Starting execution at: ${entryPoint}`);
        
        // Initialize with game config
        const gameConfig = {
            Width: 320,
            Height: 640,
            Parent: 'game-container',
            BackgroundColor: '#3d4042'
        };
        
        this.initializePhaser(gameConfig);
    }

    callMethod(methodName, args = []) {
        const address = this.methodAddresses.get(methodName);
        if (address === undefined) {
            console.warn(`Method not found: ${methodName}`);
            return;
        }

        // Push arguments onto stack
        for (let i = args.length - 1; i >= 0; i--) {
            this.stack.push(args[i]);
        }

        // Save current state
        this.callStack.push(this.pc);
        this.pc = address;

        // Execute instructions
        this.executeInstructions();
    }

    executeInstructions() {
        while (this.pc < this.instructions.length) {
            const instruction = this.instructions[this.pc];
            
            switch (instruction.OpCode) {
                case 'LOAD_CONST':
                    this.stack.push(this.parseValue(instruction.Operand));
                    break;
                    
                case 'STORE_VAR':
                    const value = this.stack.pop();
                    this.variables.set(instruction.Operand, value);
                    break;
                    
                case 'LOAD_VAR':
                    const varValue = this.variables.get(instruction.Operand);
                    this.stack.push(varValue);
                    break;
                    
                case 'CALL_METHOD':
                    this.handleMethodCall(instruction.Operand);
                    break;
                    
                case 'CALL_CONSTRUCTOR':
                    this.handleConstructorCall(instruction.Operand);
                    break;
                    
                case 'LOAD_PROPERTY':
                    this.handlePropertyLoad(instruction.Operand);
                    break;
                    
                case 'STORE_PROPERTY':
                    this.handlePropertyStore(instruction.Operand);
                    break;
                    
                case 'EXPRESSION':
                    this.handleExpression(instruction.Operand);
                    break;
                    
                case 'RETURN':
                    if (this.callStack.length > 0) {
                        this.pc = this.callStack.pop();
                        return;
                    } else {
                        return; // End of execution
                    }
                    
                default:
                    console.warn(`Unknown opcode: ${instruction.OpCode}`);
            }
            
            this.pc++;
        }
    }

    handleMethodCall(methodName) {
        console.log(`Calling method: ${methodName}`);
        
        // Handle special Phaser-related methods
        if (methodName === 'Console.WriteLine') {
            const message = this.stack.pop();
            console.log(message);
        } else if (methodName.includes('SceneManager.Start')) {
            const sceneName = this.stack.pop();
            console.log(`Starting scene: ${sceneName}`);
        } else if (methodName.includes('Add.Image')) {
            const texture = this.stack.pop();
            const y = this.stack.pop();
            const x = this.stack.pop();
            
            if (this.currentScene) {
                const image = this.currentScene.add.image(x, y, texture);
                const objId = this.nextObjectId++;
                this.gameObjects.set(objId, image);
                this.stack.push(objId);
            }
        } else if (methodName.includes('Add.Rectangle')) {
            const height = this.stack.pop();
            const width = this.stack.pop();
            const y = this.stack.pop();
            const x = this.stack.pop();
            
            if (this.currentScene) {
                const rect = this.currentScene.add.rectangle(x, y, width, height);
                const objId = this.nextObjectId++;
                this.gameObjects.set(objId, rect);
                this.stack.push(objId);
            }
        } else if (methodName.includes('Load.Image')) {
            const url = this.stack.pop();
            const key = this.stack.pop();
            console.log(`Loading image: ${key} from ${url}`);
        } else if (methodName.includes('Load.SetPath')) {
            const path = this.stack.pop();
            console.log(`Setting asset path: ${path}`);
        }
    }

    handleConstructorCall(className) {
        console.log(`Creating instance of: ${className}`);
        
        if (className === 'Player') {
            const texture = this.stack.pop() || 'pixel-man';
            const y = this.stack.pop();
            const x = this.stack.pop();
            const scene = this.stack.pop();
            
            // Create a player object
            const player = this.createPlayer(x, y, texture);
            const objId = this.nextObjectId++;
            this.gameObjects.set(objId, player);
            this.variables.set('Player', objId);
            this.stack.push(objId);
        }
    }

    createPlayer(x, y, texture) {
        if (!this.currentScene) return null;
        
        // Create a colored rectangle as the player sprite since we can't load images
        const sprite = this.currentScene.add.rectangle(x, y, 48, 48, 0xff0000); // Red square, larger
        sprite.setStrokeStyle(3, 0xffffff); // White border, thicker
        sprite.setDepth(100); // Make sure it's on top
        
        // Add player-specific properties and methods
        const player = {
            sprite: sprite,
            x: x,
            y: y,
            speed: 6,
            tileSize: 64,
            moving: false,
            targetX: 0,
            targetY: 0,
            moveDir: { dx: 0, dy: 0 },
            keyPressDelayFrames: 4,
            keyPressFrameCount: 0,
            startX: 0,
            startY: 0,
            moveProgress: 0,
            
            snapToGrid: function() {
                this.x = Math.round(this.x / this.tileSize) * this.tileSize;
                this.y = Math.round(this.y / this.tileSize) * this.tileSize;
                this.sprite.x = this.x;
                this.sprite.y = this.y;
            },
            
            getMoveDirection: function(cursors) {
                let dx = 0, dy = 0;
                if (cursors.left.isDown) dx += -1;
                if (cursors.right.isDown) dx += 1;
                if (cursors.up.isDown) dy += -1;
                if (cursors.down.isDown) dy += 1;

                if (dx !== 0 || dy !== 0) {
                    this.keyPressFrameCount += 1;
                } else {
                    this.keyPressFrameCount = 0;
                }

                if (this.keyPressFrameCount >= this.keyPressDelayFrames) {
                    return { dx, dy };
                }

                return { dx: 0, dy: 0 };
            },
            
            update: function(time, delta) {
                if (!this.moving) {
                    const moveDir = this.getMoveDirection(window.runtime.cursors);
                    
                    if (moveDir.dx !== 0 || moveDir.dy !== 0) {
                        this.moveDir = moveDir;
                        this.startX = Math.round(this.x / this.tileSize) * this.tileSize;
                        this.startY = Math.round(this.y / this.tileSize) * this.tileSize;
                        this.targetX = this.startX + moveDir.dx * this.tileSize;
                        this.targetY = this.startY + moveDir.dy * this.tileSize;
                        this.moveProgress = 0;
                        this.moving = true;
                    }
                }
                
                if (this.moving) {
                    let moveSteps = this.tileSize;
                    if (this.moveDir.dx !== 0 && this.moveDir.dy !== 0) {
                        moveSteps += moveSteps * Math.SQRT1_2;
                    }
                    this.moveProgress += (this.speed / moveSteps) * (delta / 16);
                    
                    if (this.moveProgress >= 1) {
                        this.x = this.targetX;
                        this.y = this.targetY;
                        this.moving = false;
                    } else {
                        this.x = this.lerp(this.startX, this.targetX, this.moveProgress);
                        this.y = this.lerp(this.startY, this.targetY, this.moveProgress);
                    }
                    
                    this.sprite.x = this.x;
                    this.sprite.y = this.y;
                }
            },
            
            lerp: function(a, b, t) {
                return a + (b - a) * t;
            }
        };
        
        player.snapToGrid();
        return player;
    }

    handlePropertyLoad(property) {
        console.log(`Loading property: ${property}`);
        // Handle property access
    }

    handlePropertyStore(property) {
        const value = this.stack.pop();
        console.log(`Storing property: ${property} = ${value}`);
        // Handle property assignment
    }

    handleExpression(expression) {
        console.log(`Executing expression: ${expression}`);
        // Handle general expressions
    }

    parseValue(operand) {
        if (!operand) return null;
        
        // Try to parse as number
        if (!isNaN(operand)) {
            return parseFloat(operand);
        }
        
        // Try to parse as boolean
        if (operand === 'true') return true;
        if (operand === 'false') return false;
        
        // Remove quotes from strings
        if (operand.startsWith('"') && operand.endsWith('"')) {
            return operand.slice(1, -1);
        }
        
        // Return as-is for other values
        return operand;
    }
}

// Global runtime instance
window.runtime = new CSharpBytecodeRuntime();

// Function to load and execute bytecode
async function loadAndExecuteGame(bytecodeUrl) {
    try {
        const response = await fetch(bytecodeUrl);
        const bytecodeJson = await response.text();
        
        const entryPoint = window.runtime.loadBytecode(bytecodeJson);
        window.runtime.execute(entryPoint);
        
        console.log('Game started successfully!');
    } catch (error) {
        console.error('Failed to load or execute game:', error);
    }
}

// Export for use in HTML
window.loadAndExecuteGame = loadAndExecuteGame;
