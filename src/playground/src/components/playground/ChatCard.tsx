import {
    makeStyles,
    Body1,
    Button,
    CardFooter,
    Textarea,
    Spinner,
    shorthands
} from "@fluentui/react-components";
import { Dispatch, useEffect, useRef, useState } from "react";
import { Delete24Regular, SendRegular, Attach24Regular } from "@fluentui/react-icons";
import { Message } from "./Message";
import { Response } from "./Response";
import { useEventDataContext } from "../../providers/EventDataProvider";
import { Card } from "./Card";
import { ChatResponseMessageExtended } from "../../pages/playground/Chat.state";

interface CardProps {
    onPromptEntered: Dispatch<string>;
    messageList: ChatResponseMessageExtended[];
    onClear: () => void;
    isLoading: boolean;
    canChat: boolean;
}

const useStyles = makeStyles({
    dialog: {
        display: "block",
    },
    buttonContainer: {
        display: "flex",
        justifyContent: "flex-end",  // Align buttons to the right
    },
    smallButton: {
        marginBottom: "12px",
        ...shorthands.margin("4px"),
    },
    startCard: {
        display: "flex",
        maxWidth: "80%",
        ...shorthands.margin("35%", "20%"),
        ...shorthands.padding("20px", "0px"),
    },
    chatCard: {
        display: "flex",
        height: "calc(100vh - 100px)",
    },
    wrapper: {
        display: "flex",
        flexDirection: "column",
        justifyContent: "flex-end", // Ensure content sticks to the bottom
        height: "120px",
        maxHeight: "120px",
    },
    userQuery: {
        flexGrow: 0,                // Do not grow to fill remaining space
        marginBottom: "10px",
        overflowY: "auto",          // Scroll if content overflows
    }
});

export const ChatCard = ({
    onPromptEntered,
    messageList,
    onClear,
    isLoading,
    canChat,
}: CardProps) => {
    const chat = useStyles();
    const chatContainerRef = useRef<HTMLDivElement>(null);
    const { isAuthorized } = useEventDataContext();

    useEffect(() => {
        if (chatContainerRef.current) {
            chatContainerRef.current.scrollTop =
                chatContainerRef.current.scrollHeight;
        }
    }, [messageList]);

    return (
        <Card header="Chat session" className={chat.chatCard} >

            {isAuthorized && (
                <>
                    <div
                        id={"chatContainer"}
                        style={{ overflowY: "auto" }}
                        ref={chatContainerRef}
                    >

                        {messageList.length > 1 ? (
                            messageList.map((message, index) => {
                                if (message.role === "system") {
                                    return null;
                                }
                                return message.role === "user" ? (
                                    <Message key={index} message={message} />
                                ) : (
                                    <Response key={index} message={message} />
                                );
                            })
                        ) : (
                            <Card className={chat.startCard}>
                                <Body1 style={{ textAlign: "center" }}>
                                    {!canChat && (<h2>Select a model</h2>)}
                                    {canChat && (
                                        <>
                                            <h2>Start chatting</h2>
                                            Test your assistant by sending queries below. Then adjust your assistant setup to improve the assistant's responses.
                                        </>
                                    )}
                                </Body1>
                            </Card>
                        )}
                    </div>
                    {isLoading && <Spinner />}
                    {isAuthorized && (
                        <ChatInput
                            promptSubmitted={onPromptEntered}
                            onClear={onClear}
                            canChat={canChat}
                        />
                    )}
                    {!isAuthorized && (
                        <>
                            <CardFooter style={{ height: "10vh" }}>
                                <p>Please enter your event code to start the chat session.</p>
                            </CardFooter>
                        </>
                    )}
                </>
            )}
        </Card>
    );
};

const hasPrompt = (prompt: string) => {
    const regex = /^\s*$/;
    return !regex.test(prompt);
};

function ChatInput({
    promptSubmitted,
    onClear,
    canChat,
}: {
    promptSubmitted: Dispatch<string>;
    onClear: () => void;
    canChat: boolean;
}) {
    const [userPrompt, setPrompt] = useState("");
    const [files, setFiles] = useState<{ name: string, dataUrl: string }[]>([]);
    const maxFiles = 10;

    const chat = useStyles();

    // Handle file selection and convert files to base64 data URLs
    const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const selectedFiles = Array.from(e.target.files || []);

        if (selectedFiles.length + files.length > maxFiles) {
            alert(`You can only upload up to ${maxFiles} files.`);
            return;
        }

        try {
            const newFiles = await Promise.all(
                selectedFiles.map(file => new Promise<{ name: string, dataUrl: string }>((resolve, reject) => {
                    const reader = new FileReader();

                    reader.onload = () => {
                        const dataUrl = reader.result?.toString();
                        if (dataUrl) {
                            resolve({ name: file.name, dataUrl });
                        } else {
                            reject(new Error('Failed to read file data'));
                        }
                    };

                    reader.onerror = () => {
                        console.error(`Error reading file: ${file.name}`);
                        reject(new Error(`Error reading file: ${file.name}`));
                    };

                    reader.readAsDataURL(file);
                }))
            );

            setFiles(prevFiles => [...prevFiles, ...newFiles]);
        } catch (error) {
            console.error('Error encoding files to base64:', error);
            alert('Error uploading files, please try again.');
        } finally {
            // Clear the file input value to allow re-uploading the same file
            if (fileInputRef.current) {
                fileInputRef.current.value = '';
            }
        }
    };

    const fileInputRef = useRef<HTMLInputElement | null>(null);

    const triggerFileInput = () => {
        if (fileInputRef.current) {
            fileInputRef.current.click();
        }
    };

    const handleSend = () => {
        // Prepare prompt data based on whether files exist
        const promptData = files?.length
            ? [
                { type: "text", text: userPrompt },
                ...files.map(file => ({
                    type: "image_url",
                    imageUrl: { url: file.dataUrl }
                }))
            ]
            : userPrompt;

        promptSubmitted(promptData); // Pass the constructed payload or string to the parent
        setPrompt(""); // Clear the prompt
    };

    const clearAll = () => {
        setFiles([]);  // Clear all uploaded files
        onClear();
        setPrompt("");
    };

    return (
        <div className={chat.wrapper}>
            <Textarea className={chat.wrapper}
                value={userPrompt}
                placeholder="Type user query here (Shift + Enter for new line)"
                disabled={!canChat}
                onChange={(event) => setPrompt(event.target.value)}
                onKeyDown={(event) => {
                    if (
                        event.key === "Enter" &&
                        !event.shiftKey &&
                        hasPrompt(userPrompt)
                    ) {
                        handleSend();
                        setPrompt("");
                        event.preventDefault();
                    }
                }}
            />
            <div className={chat.buttonContainer}>
                {/* Hidden file input */}
                <input
                    ref={fileInputRef}
                    id="upload-button"
                    type="file"
                    accept="image/png, image/jpeg, image/jpg, image/gif, image/webp"
                    onChange={handleFileChange}
                    style={{ display: 'none' }} // This hides the file input
                    multiple // Enable multiple file selection
                />

                {/* Button to trigger the file input */}
                <Button
                    className={chat.smallButton}
                    id="upload-button"
                    icon={<Attach24Regular />}
                    iconPosition="before"
                    disabled={!canChat}
                    onClick={triggerFileInput}>
                    {files?.length} of {maxFiles}
                </Button>
                <Button
                    className={chat.smallButton}
                    id="clear-button"
                    disabled={!canChat}
                    icon={<Delete24Regular />}
                    iconPosition="before"
                    onClick={clearAll}
                >
                </Button>
                <Button
                    className={chat.smallButton}
                    id={"send-button"}
                    icon={<SendRegular />}
                    iconPosition="before"
                    appearance="primary"
                    onClick={handleSend} // Use handleSend function to check for file or prompt
                    disabled={!canChat || !hasPrompt(userPrompt)} // Disable if no input or file
                >
                </Button>
            </div>
        </div>
    );
}
