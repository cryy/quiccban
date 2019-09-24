import * as React from 'react';
import { Typography, makeStyles, Theme, createStyles, Paper, List } from '@material-ui/core';
import { HookProps } from '../../props/HookProps';
import { ICase } from '../../entities/guild/ICase';
import CasePreview from './CasePreview';
import Preloader from '../Preloader';

const useStyle = makeStyles((theme: Theme) =>
    createStyles({
        divider: {
            marginTop: "3px",
            marginBottom: "4px"
        },
        loader: {
            position: "absolute",
            left: "50%",
            marginLeft: "-30px",
            marginTop: "3px"
        },
        subtext: {
            fontSize: "24px",
            textAlign: "center",
            color: theme.palette.text.hint,
            marginBottom: "8px"
        },
        guildIcon: {
            verticalAlign: "middle",
            width: '40px',
            height: '40px'
        },
        list: {
            width: "100%",
            backgroundColor: theme.palette.background.paper
        },
        date: {
            fontSize: '13px',
            marginTop: '2px',
            marginBottom: '-8px'
        },
        pointer: {
            cursor: 'pointer'
        },
        previewWrap: {
            wordWrap: 'break-word'
        }
    }),
);

export default function RecentCases(props: HookProps) {

    const [recents, setRecents] = React.useState<ICase[]>(null);
    const classes = useStyle({});

    React.useEffect(() => {


        let localRecents: ICase[];


        props.apiClient.getRecentCases()
            .then(cases => {
                setTimeout(() => {
                    setRecents(cases);
                    localRecents = cases;
                }, 900);

            });

        let subId = props.apiClient.onNewCase(c => {
            let newArr = [c, ...localRecents.slice(0, 9)];
            setRecents(newArr);
            localRecents = newArr;
        });


        return () => {
            props.apiClient.removeListener(subId);
        }
    }, []);


    function assembleCases() {
        return (
            <div>
                <Typography className={classes.subtext}>These are the most recent cases:</Typography>
                <Paper>
                    <List className={classes.list}>
                        {recents.map((c, i, arr) => {
                            let divider = i < arr.length - 1;
                            return <CasePreview case={c} divider={divider} classes={classes} {...props} />
                        })}
                    </List>
                </Paper>
            </div>
        );
    }

    let el;

    if (!recents)
        el = <Preloader iclass={classes.loader} size={60} />;
    else if (recents.length === 0)
        el = <Typography className={classes.subtext}>There are no recent cases :(</Typography>;
    else
        el = assembleCases();

    return el;

}
