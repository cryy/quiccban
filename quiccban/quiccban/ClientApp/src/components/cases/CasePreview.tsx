import * as React from 'react';
import { CasePreviewProps } from "../../props/CasePreviewProps";
import { ListItem, ListItemAvatar, Avatar, ListItemText, Link, Typography, Divider } from '@material-ui/core';
import { toPassive } from '../../entities/enums/ActionType';


export default function CasePreview(props: CasePreviewProps) {
    let c = props.case;
    let date = new Date(Number(c.unixTimestamp)).toLocaleString("en-GB", { hour12: false, year: 'numeric', month: 'long', day: 'numeric', hour: 'numeric', minute: 'numeric', second: 'numeric' });
    let guild = props.user.guilds.filter(x => x.id == c.guild.id)[0];




    return (
        <div>
            <ListItem alignItems="flex-start" onClick={(e) => { }} className={props.classes.pointer}>
                <ListItemAvatar>
                    <Avatar alt={guild.name} src={guild.iconUrl ? guild.iconUrl : ""} className={props.classes.guildIcon}>{guild.name[0]}</Avatar>
                </ListItemAvatar>
                <ListItemText className={props.classes.previewWrap} primary={guild.name} secondary={
                    <React.Fragment><Link onClick={(e) => { e.stopPropagation(); }}>{c.issuerUser ? (c.issuerUser.username + "#" + c.issuerUser.discriminator) : c.issuerId}</Link> {toPassive(c.actionType)} <Link>{c.targetUser ? (c.targetUser.username + "#" + c.targetUser.discriminator) : c.targetId}</Link>{(c.reason ? ` for "${c.reason}"` : "")}.
                        <Typography component="p" className={props.classes.date}>{date}</Typography>
                    </React.Fragment>
                }>
                </ListItemText>
            </ListItem>
            {props.divider ? <Divider variant="inset" component="li" /> : ""}
        </div>
    );
}